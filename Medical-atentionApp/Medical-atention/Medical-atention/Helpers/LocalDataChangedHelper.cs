using Xamarin.Forms;

namespace Medical_atention.Helpers
{
    public static class LocalDataChangedHelper
    {
        public const string PatientsChangedMessage = "LocalPatientsChanged";

        public static void NotifyPatientsChanged()
        {
            MessagingCenter.Send(typeof(LocalDataChangedHelper), PatientsChangedMessage);
        }
    }
}
