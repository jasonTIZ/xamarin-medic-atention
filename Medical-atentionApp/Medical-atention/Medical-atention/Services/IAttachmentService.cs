using Medical_atention.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface IAttachmentService
    {
        Task<List<AttachmentItem>> LoadForConsultationAsync(int consultationLocalId, int? consultationServerId, string token);
        Task<(AttachmentItem item, string error)> AddFromGalleryAsync(int consultationLocalId, int? consultationServerId, string token, IProgress<double> progress);
        Task<(AttachmentItem item, string error)> AddFromCameraAsync(int consultationLocalId, int? consultationServerId, string token, IProgress<double> progress);
        Task<(byte[] compressed, string fileName, string error)> PickAndCompressAsync(bool fromCamera);
        Task<AttachmentItem> AddBytesAsync(int consultationLocalId, int? consultationServerId, byte[] compressed, string fileName, string token, IProgress<double> progress);
        Task<string> DeleteAsync(AttachmentItem item, string token);
        Task<string> SaveToGalleryAsync(AttachmentItem item);
    }
}
