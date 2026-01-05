using NLog;
using XSNotifications;

namespace JustNotification
{
    class XSNotifications
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static XSNotifier XSOverlay = new();

        public static void Show(string titleText, string bodyText, string nameText,string appIcon)
        {
            XSNotification notification = new();
            notification.Title = titleText ?? "";
            notification.Content = bodyText ?? "";
            notification.Timeout = Properties.Settings.Default.timeout / 1000f;

            logger.Debug($"[XSNotifications] Creating notification - Title: '{titleText}', Body: '{bodyText}', App: '{nameText}', HasIcon: {appIcon != ""}");

            // 設定でソフト名を有効にしていて、なおかつソフト名がnullでなければソフト名を付け足す
            if (Properties.Settings.Default.enable_title && nameText != null)
            {
                notification.Title += $" - {nameText ?? ""}";
                logger.Trace($"[XSNotifications] Title updated with app name: '{notification.Title}'");
            }

            // アイコンが使用できればアイコンを付ける
            if (appIcon != "")
            {
                notification.UseBase64Icon = true;
                notification.Icon = appIcon ?? "";
                logger.Trace($"[XSNotifications] Icon attached (Base64 length: {appIcon.Length})");
            }

            logger.Debug($"[XSNotifications] Sending to XSOverlay - Final Title: '{notification.Title}', Content: '{notification.Content}', Timeout: {notification.Timeout}s");
            XSOverlay.SendNotification(notification);
        }
    }
}
