using Xamarin.Forms;

namespace Medical_atention.Helpers
{
    public static class SyncNotificationHelper
    {
        public const string SyncCompletedMessage = "SyncCompleted";

        public static void NotifyCompleted(int syncedCount)
        {
            if (syncedCount <= 0) return;
            MessagingCenter.Send(typeof(SyncNotificationHelper), SyncCompletedMessage, syncedCount);
        }
    }
}
