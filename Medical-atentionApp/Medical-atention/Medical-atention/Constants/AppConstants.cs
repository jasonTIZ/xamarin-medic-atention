namespace Medical_atention.Constants
{
    public static class AppConstants
    {
        // Android emulator → host machine localhost
        // For iOS Simulator change to: http://localhost:5258
        // For physical device change to: http://<your-local-ip>:5258
        public const string ApiBaseUrl = "http://10.0.2.2:5258";

        public const string TokenKey    = "auth_token";
        public const string UserIdKey   = "user_id";
        public const string UserNameKey = "user_name";
        public const string UserRoleKey = "user_role";
    }
}
