using Medical_atention.Constants;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Helpers
{
    public static class LastSyncHelper
    {
        public static Task RecordPatientSyncAsync()
        {
            return SecureStorage.SetAsync(
                AppConstants.LastPatientSyncKey,
                DateTime.UtcNow.ToString("o"));
        }
    }
}
