using System;
using System.Collections.Generic;

using Foundation;
using UIKit;
using UserNotifications;
using Firebase.CloudMessaging;
using Medical_atention.Services;

namespace Medical_atention.iOS
{
    // The UIApplicationDelegate for the application. Responsable de lanzar la UI y de
    // integrar las notificaciones push (APNs + Firebase Cloud Messaging).
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate,
        IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            // Inicializa Firebase (lee GoogleService-Info.plist del bundle).
            Firebase.Core.App.Configure();

            // Solicita permiso de notificaciones; maneja el rechazo sin romper la app.
            UNUserNotificationCenter.Current.Delegate = this;
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                (granted, error) =>
                {
                    if (!granted)
                        System.Diagnostics.Debug.WriteLine(
                            "[AppDelegate] Permiso de notificaciones denegado por el usuario.");
                });

            Messaging.SharedInstance.Delegate = this;
            app.RegisterForRemoteNotifications();

            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }

        // APNs token recibido → se lo pasamos a Firebase.
        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            Messaging.SharedInstance.ApnsToken = deviceToken;
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            System.Diagnostics.Debug.WriteLine($"[AppDelegate] Error registrando APNs: {error?.LocalizedDescription}");
        }

        // Token FCM (refresco incluido).
        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            if (!string.IsNullOrEmpty(fcmToken))
                PushNotificationService.Instance.OnTokenReceived(fcmToken, "ios");
        }

        // Notificación en primer plano → alerta.
        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification,
            Action<UNNotificationPresentationOptions> completionHandler)
        {
            var content = notification.Request.Content;
            PushNotificationService.Instance.HandleForegroundMessage(
                content.Title, content.Body, ToDictionary(content.UserInfo));
            completionHandler(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound);
        }

        // Notificación abierta (tap) → navegación.
        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center,
            UNNotificationResponse response, Action completionHandler)
        {
            var userInfo = response.Notification.Request.Content.UserInfo;
            PushNotificationService.Instance.HandleNotificationOpened(ToDictionary(userInfo));
            completionHandler();
        }

        private static Dictionary<string, string> ToDictionary(NSDictionary userInfo)
        {
            var dict = new Dictionary<string, string>();
            if (userInfo == null) return dict;

            foreach (var key in userInfo.Keys)
            {
                var k = key?.ToString();
                if (string.IsNullOrEmpty(k)) continue;
                dict[k] = userInfo[key]?.ToString();
            }
            return dict;
        }
    }
}
