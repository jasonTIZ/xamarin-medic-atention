using Medical_atention.Constants;
using Medical_atention.Data;
using Medical_atention.Models;
using Medical_atention.Models.Entities;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Services
{
    public class AttachmentService : IAttachmentService
    {
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        private readonly LocalDatabase _localDb;

        private const int MaxWidth = 800;
        private const int JpegQuality = 70;

        public AttachmentService() : this(LocalDatabase.Instance) { }

        public AttachmentService(LocalDatabase localDb)
        {
            _localDb = localDb;
        }

        public async Task<List<AttachmentItem>> LoadForConsultationAsync(
            int consultationLocalId, int? consultationServerId, string token)
        {
            var local = await _localDb.GetAttachmentsByConsultationAsync(consultationLocalId, consultationServerId);
            var items = local.Select(ToItem).ToList();

            if (consultationServerId.HasValue && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var req = BuildRequest(HttpMethod.Get,
                        $"/api/consultations/{consultationServerId}/attachments", token);
                    var response = await _client.SendAsync(req);

                    if (response.IsSuccessStatusCode)
                    {
                        var dtos = JsonConvert.DeserializeObject<List<AttachmentResponseDto>>(
                            await response.Content.ReadAsStringAsync()) ?? new List<AttachmentResponseDto>();

                        foreach (var dto in dtos)
                        {
                            var existing = items.FirstOrDefault(i => i.ServerId == dto.Id);
                            if (existing != null)
                            {
                                existing.RemoteUrl = dto.Url;
                            }
                            else
                            {
                                var entity = new AttachmentEntity
                                {
                                    ServerId = dto.Id,
                                    ConsultationLocalId = consultationLocalId,
                                    ConsultationServerId = consultationServerId,
                                    FileName = dto.FileName,
                                    RemoteUrl = dto.Url,
                                    PendingSync = false,
                                    CreatedAt = dto.UploadedAt
                                };
                                await _localDb.SaveAttachmentAsync(entity);
                                items.Add(ToItem(entity));
                            }
                        }
                    }
                }
                catch (Exception) { }
            }

            return items.OrderBy(i => i.LocalId).ToList();
        }

        public Task<(AttachmentItem item, string error)> AddFromGalleryAsync(
            int consultationLocalId, int? consultationServerId, string token, IProgress<double> progress)
            => AddImageAsync(
                () => MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "Seleccionar imagen" }),
                consultationLocalId, consultationServerId, token, progress);

        public Task<(AttachmentItem item, string error)> AddFromCameraAsync(
            int consultationLocalId, int? consultationServerId, string token, IProgress<double> progress)
            => AddImageAsync(
                () => MediaPicker.CapturePhotoAsync(),
                consultationLocalId, consultationServerId, token, progress);

        public async Task<(byte[] compressed, string fileName, string error)> PickAndCompressAsync(bool fromCamera)
        {
            FileResult file;
            try
            {
                file = fromCamera
                    ? await MediaPicker.CapturePhotoAsync()
                    : await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "Seleccionar imagen" });
                if (file is null) return (null, null, null);
            }
            catch (Exception)
            {
                return (null, null, "No se pudo acceder a la cámara o galería");
            }

            try
            {
                byte[] compressed;
                using (var stream = await file.OpenReadAsync())
                    compressed = CompressImage(stream);
                return (compressed, file.FileName, null);
            }
            catch (Exception)
            {
                return (null, null, "Error al procesar la imagen");
            }
        }

        public async Task<AttachmentItem> AddBytesAsync(
            int consultationLocalId, int? consultationServerId,
            byte[] compressed, string fileName,
            string token, IProgress<double> progress)
        {
            var localPath = await SaveLocalAsync(compressed, fileName);

            var entity = new AttachmentEntity
            {
                ConsultationLocalId = consultationLocalId,
                ConsultationServerId = consultationServerId,
                FileName = fileName,
                LocalPath = localPath,
                PendingSync = true,
                CreatedAt = DateTime.UtcNow
            };
            entity.Id = await _localDb.SaveAttachmentAsync(entity);

            var item = ToItem(entity);

            if (consultationServerId.HasValue && Connectivity.NetworkAccess == NetworkAccess.Internet)
                _ = UploadInBackgroundAsync(item, entity, compressed, consultationServerId.Value, token, progress);

            return item;
        }

        public async Task<string> DeleteAsync(AttachmentItem item, string token)
        {
            if (item.ServerId.HasValue && item.ConsultationServerId.HasValue
                && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var req = BuildRequest(HttpMethod.Delete,
                        $"/api/consultations/{item.ConsultationServerId}/attachments/{item.ServerId}", token);
                    var resp = await _client.SendAsync(req);
                    if (!resp.IsSuccessStatusCode && resp.StatusCode != System.Net.HttpStatusCode.NotFound)
                        return "No se pudo eliminar del servidor";
                }
                catch (Exception)
                {
                    return "Sin conexión para eliminar";
                }
            }

            if (!string.IsNullOrEmpty(item.LocalPath) && File.Exists(item.LocalPath))
                File.Delete(item.LocalPath);

            await _localDb.DeleteAttachmentAsync(item.LocalId);
            return null;
        }

        public async Task<string> SaveToGalleryAsync(AttachmentItem item)
        {
            try
            {
                string sourcePath = item.LocalPath;

                if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                {
                    if (string.IsNullOrEmpty(item.RemoteUrl))
                        return "No hay imagen disponible";

                    sourcePath = await DownloadToTempAsync(item.RemoteUrl);
                    if (sourcePath is null) return "No se pudo descargar la imagen";
                }

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = item.FileName,
                    File = new ShareFile(sourcePath)
                });
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // ── Privados ──────────────────────────────────────────────────────────

        private async Task<(AttachmentItem item, string error)> AddImageAsync(
            Func<Task<FileResult>> picker,
            int consultationLocalId, int? consultationServerId,
            string token, IProgress<double> progress)
        {
            FileResult file;
            try
            {
                file = await picker();
                if (file is null) return (null, null);
            }
            catch (Exception)
            {
                return (null, "No se pudo acceder a la cámara o galería");
            }

            try
            {
                byte[] compressed;
                using (var stream = await file.OpenReadAsync())
                    compressed = CompressImage(stream);

                var localPath = await SaveLocalAsync(compressed, file.FileName);

                var entity = new AttachmentEntity
                {
                    ConsultationLocalId = consultationLocalId,
                    ConsultationServerId = consultationServerId,
                    FileName = file.FileName,
                    LocalPath = localPath,
                    PendingSync = true,
                    CreatedAt = DateTime.UtcNow
                };
                entity.Id = await _localDb.SaveAttachmentAsync(entity);

                var item = ToItem(entity);

                if (consultationServerId.HasValue && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    _ = UploadInBackgroundAsync(item, entity, compressed, consultationServerId.Value, token, progress);
                else
                    progress?.Report(0);

                return (item, null);
            }
            catch (Exception)
            {
                return (null, "Error al procesar la imagen");
            }
        }

        private async Task UploadInBackgroundAsync(
            AttachmentItem item, AttachmentEntity entity,
            byte[] imageBytes, int consultationServerId,
            string token, IProgress<double> progress)
        {
            try
            {
                item.IsUploading = true;
                item.UploadProgress = 0;

                var streamContent = new ProgressableStreamContent(
                    new MemoryStream(imageBytes),
                    (sent, total) => progress?.Report(total > 0 ? (double)sent / total : 0));
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                HttpResponseMessage response;
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(streamContent, "files", entity.FileName);
                    var req = new HttpRequestMessage(HttpMethod.Post,
                        AppConstants.ApiBaseUrl + $"/api/consultations/{consultationServerId}/attachments")
                    {
                        Content = content
                    };
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    response = await _client.SendAsync(req);
                }

                if (response.IsSuccessStatusCode)
                {
                    var dtos = JsonConvert.DeserializeObject<List<AttachmentResponseDto>>(
                        await response.Content.ReadAsStringAsync());
                    var dto = dtos?.FirstOrDefault();
                    if (dto != null)
                    {
                        item.ServerId = dto.Id;
                        item.RemoteUrl = dto.Url;
                        item.PendingSync = false;
                        await _localDb.UpdateAttachmentSyncedAsync(entity.Id, dto.Id, dto.Url);
                    }
                }

                progress?.Report(1);
            }
            catch (Exception) { }
            finally
            {
                item.IsUploading = false;
            }
        }

        private static byte[] CompressImage(Stream source)
        {
            using (var original = SKBitmap.Decode(source))
            {
                if (original is null) throw new InvalidOperationException("No se pudo decodificar la imagen");

                SKBitmap target;
                if (original.Width > MaxWidth)
                {
                    int h = (int)((double)original.Height * MaxWidth / original.Width);
                    target = original.Resize(new SKImageInfo(MaxWidth, h), SKFilterQuality.Medium);
                }
                else
                {
                    target = original;
                }

                using (var img = SKImage.FromBitmap(target))
                using (var data = img.Encode(SKEncodedImageFormat.Jpeg, JpegQuality))
                {
                    if (!ReferenceEquals(target, original))
                        target.Dispose();

                    return data.ToArray();
                }
            }
        }

        private static async Task<string> SaveLocalAsync(byte[] bytes, string originalName)
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "attachments");
            Directory.CreateDirectory(dir);

            var ext = Path.GetExtension(originalName);
            if (string.IsNullOrEmpty(ext)) ext = ".jpg";
            var path = Path.Combine(dir, $"{Guid.NewGuid()}{ext}");

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                await fs.WriteAsync(bytes, 0, bytes.Length);
            return path;
        }

        private static async Task<string> DownloadToTempAsync(string url)
        {
            try
            {
                var bytes = await _client.GetByteArrayAsync(url);
                var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static AttachmentItem ToItem(AttachmentEntity e) => new AttachmentItem
        {
            LocalId = e.Id,
            ServerId = e.ServerId,
            ConsultationServerId = e.ConsultationServerId,
            LocalPath = e.LocalPath,
            RemoteUrl = e.RemoteUrl,
            FileName = e.FileName,
            PendingSync = e.PendingSync
        };

        private static HttpRequestMessage BuildRequest(HttpMethod method, string path, string token)
        {
            var req = new HttpRequestMessage(method, AppConstants.ApiBaseUrl + path);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return req;
        }

        // ── Helper para progreso de upload ─────────────────────────────────────

        private class ProgressableStreamContent : HttpContent
        {
            private readonly Stream _source;
            private readonly Action<long, long> _onProgress;

            public ProgressableStreamContent(Stream source, Action<long, long> onProgress)
            {
                _source = source;
                _onProgress = onProgress;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context)
            {
                var buffer = new byte[8192];
                long total = _source.CanSeek ? _source.Length : -1;
                long sent = 0;
                int read;

                while ((read = await _source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, read);
                    sent += read;
                    _onProgress?.Invoke(sent, total);
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                length = _source.CanSeek ? _source.Length : -1;
                return _source.CanSeek;
            }
        }

        // DTO local para deserializar respuesta del API
        private class AttachmentResponseDto
        {
            public int Id { get; set; }
            public int ConsultationId { get; set; }
            public string FileName { get; set; }
            public string Url { get; set; }
            public long FileSizeBytes { get; set; }
            public DateTime UploadedAt { get; set; }
        }
    }
}
