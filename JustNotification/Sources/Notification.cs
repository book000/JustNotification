using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace JustNotification
{
    class Notification
    {
        public static bool AccessAllowed { get; private set; } = false;
        public static bool IsEnableGetNotification = true;
        private static UserNotificationListener userNotificationListener = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static async void Init()
        {
            try
            {
                if (!ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
                {
                    AccessAllowed = false;
                    userNotificationListener = null;
                    return;
                }

                userNotificationListener = UserNotificationListener.Current;
                UserNotificationListenerAccessStatus accessStatus = await userNotificationListener.RequestAccessAsync();

                if (accessStatus != UserNotificationListenerAccessStatus.Allowed)
                {
                    AccessAllowed = false;
                    userNotificationListener = null;
                    logger.Warn("Notification listener access not allowed: {0}", accessStatus);
                    return;
                }
                AccessAllowed = true;

                GetNotification();
            }
            catch (Exception ex)
            {
                AccessAllowed = false;
                userNotificationListener = null;
                logger.Error(ex, "Notification.Init failed");
            }
        }

        private static async void GetNotification()
        {
            List<uint> notificationIds = new();
            bool init = false;
            bool notSupportedLogged = false;

            while (IsEnableGetNotification)
            {
                try
                {
                    if (userNotificationListener == null)
                    {
                        AccessAllowed = false;
                        await Task.Delay(1000);
                        continue;
                    }

                    IReadOnlyList<UserNotification> userNotifications = await userNotificationListener.GetNotificationsAsync(NotificationKinds.Toast);

                    // 初回取得時点で既にある通知は投げないようにする
                    if (!init)
                    {
                        foreach (var n in userNotifications) notificationIds.Add(n.Id);
                        init = true;
                    }

                    foreach (var n in userNotifications)
                    {
                        if (!notificationIds.Contains(n.Id))
                        {
                            ShowNotification(n);
                            notificationIds.Add(n.Id);
                        }
                    }

                    await Task.Delay(Properties.Settings.Default.interval);
                }
                catch (NotImplementedException ex)
                {
                    // 環境/OS/実行形態によっては UserNotificationListener の一部 API が E_NOTIMPL になる。
                    // この場合はリトライしても改善しないため、通知取得機能自体を無効化してログスパムを防ぐ。
                    AccessAllowed = false;
                    userNotificationListener = null;
                    IsEnableGetNotification = false;

                    if (!notSupportedLogged)
                    {
                        notSupportedLogged = true;
                        logger.Warn(ex, "UserNotificationListener.GetNotificationsAsync is not supported on this environment. Notification polling disabled.");
                    }

                    return;
                }
                catch (Exception ex)
                {
                    // WinRT/権限/一時的な失敗等で例外が飛ぶことがあるため、落とさずログに残す
                    logger.Error(ex, "Notification polling failed");
                    await Task.Delay(2000);
                }
            }
        }

        private static void ShowNotification(UserNotification n)
        {
            var notificationBinding = n.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            if (notificationBinding != null)
            {
                IReadOnlyList<AdaptiveNotificationText> textElements = notificationBinding.GetTextElements();

                string nameText = n.AppInfo.DisplayInfo.DisplayName;
                string titleText = textElements.FirstOrDefault()?.Text;
                string bodyText = string.Join("\n", textElements.Skip(1).Select(t => t.Text));

                logger.Trace($"NotificationDetected: {nameText}");

                NotificationHandler.Show(titleText, bodyText, nameText);

            }
        }
    }
}
