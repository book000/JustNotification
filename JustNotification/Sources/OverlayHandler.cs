using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Windows.Forms;
using NLog;

namespace JustNotification
{
    class OverlayHandler
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void Init()
        {
            // XSOverlay 経由で通知を出す設定の場合、外部オーバーレイ(JustNotificationOverlay.exe)は不要
            if (Properties.Settings.Default.use_xsoverlay)
            {
                logger.Info("use_xsoverlay=true; skip starting JustNotificationOverlay.exe");
                return;
            }

            string AppPath = Path.GetFullPath("./overlay/JustNotificationOverlay.exe");

            // ファイルチェック
            if (!File.Exists(AppPath))
            {
                // リポジトリ/ビルド構成によっては同梱されていないため、XSOverlay方式へ自動フォールバック
                logger.Warn("JustNotificationOverlay.exe not found at {0}. Falling back to XSOverlay.", AppPath);

                try
                {
                    Properties.Settings.Default.use_xsoverlay = true;
                    Properties.Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to persist use_xsoverlay fallback setting");
                }

                // ここでダイアログを出すと毎回邪魔なのでログのみ。
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = AppPath,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to start overlay process: {0}", AppPath);
                MessageBox.Show(
                    "オーバーレイの起動に失敗しました。\n\n" +
                    $"Path: {AppPath}\n\n" +
                    ex.Message,
                    "オーバーレイエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
