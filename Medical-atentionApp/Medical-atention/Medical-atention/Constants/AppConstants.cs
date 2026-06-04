using Xamarin.Essentials;

namespace Medical_atention.Constants
{
    public static class AppConstants
    {
        // Android Emulator → laptop host
        public const string ApiBaseUrlEmulator = "http://10.0.2.2:5258";

        // Physical device (same Wi‑Fi as laptop)
        public const string ApiBaseUrlPhysical = "http://192.168.100.12:5258";

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
    }
}
