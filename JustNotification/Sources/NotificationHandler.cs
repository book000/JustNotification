using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;

namespace JustNotification
{
    class NotificationHandler
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private struct JNMessage
        {
            public float timeout { get; set; }
            public string title { get; set; }
            public string content { get; set; }
        }

        private static IPC socket = new();

        public static void Show(string titleText, string bodyText, string nameText)
        {
            JNMessage notification = new JNMessage();

            notification.title = titleText ?? "";
            notification.content = bodyText ?? "";
            notification.timeout = Properties.Settings.Default.timeout / 1000f;

            // 設定でソフト名を有効にしていて、なおかつソフト名がnullでなければソフト名を付け足す
            if (Properties.Settings.Default.enable_title && nameText != null)
            {
                notification.title += $" - {nameText ?? ""}";
            }

            string json = JsonSerializer.Serialize(notification);

            // XSOverlay 経由で通知を出す場合は NamedPipe/外部オーバーレイ不要
            if (Properties.Settings.Default.use_xsoverlay)
            {
                try
                {
                    // タイトル文字列はここで既にソフト名を付与済みなので、XSNotifications側では付与しない
                    XSNotifications.Show(notification.title, notification.content, null, "");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to send notification via XSOverlay");
                }

                return;
            }

            // 外部オーバーレイ(JustNotificationOverlay.exe)へ送信
            _ = SendViaPipeSafeAsync(json);
        }

        private static async Task SendViaPipeSafeAsync(string json)
        {
            try
            {
                await socket.Send(json);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to send notification via NamedPipe (overlay not running?)");
            }
        }
    }
}
