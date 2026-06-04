using Xamarin.Essentials;

namespace Medical_atention.Constants
{
    public static class AppConstants
    {
        // Android Emulator → laptop host
        public const string ApiBaseUrlEmulator = "http://10.0.2.2:5258";

        // Physical device: en Debug usa 127.0.0.1 + `adb reverse tcp:5258 tcp:5258` (USB).
        // En Release o Wi‑Fi sin reverse, ajusta la IP de tu red o compila con la tuya en local.
#if DEBUG
        public const string ApiBaseUrlPhysical = "http://127.0.0.1:5258";
#else
        public const string ApiBaseUrlPhysical = "http://192.168.100.54:5258";
#endif

        // iOS Simulator (if applicable)
        public const string ApiBaseUrlIosSimulator = "http://localhost:5258";

        public static string ApiBaseUrl
        {
            get
            {
                if (DeviceInfo.DeviceType == DeviceType.Virtual)
                {
                    return DeviceInfo.Platform == DevicePlatform.iOS
                        ? ApiBaseUrlIosSimulator
                        : ApiBaseUrlEmulator;
                }

                return ApiBaseUrlPhysical;
            }
        }

        public const string TokenKey = "auth_token";
        public const string UserIdKey = "user_id";
        public const string UserNameKey = "user_name";
        public const string UserRoleKey = "user_role";
        public const string LastPatientSyncKey = "last_patient_sync";
    }
}
