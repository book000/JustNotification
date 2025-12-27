using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using NLog;
using Valve.VR;

namespace JustNotification
{
    class SteamVR
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static EVRInitError EVRErr;
        private static CVRSystem OVR;
        private static CVRApplications OVRApp;

        private static string path = Path.GetFullPath("./Resources/JustNotification.vrmanifest");
        private static string pchkey = "JustNotification";

        public static void Init()
        {
            try
            {
                EVRErr = new();
                OVR = OpenVR.Init(ref EVRErr, EVRApplicationType.VRApplication_Utility);
            }
            catch (DllNotFoundException ex)
            {
                logger.Error(ex, "OpenVR native DLL not found");
                MessageBox.Show(
                    "OpenVR の DLL が見つからないため SteamVR 初期化に失敗しました。\n" +
                    "openvr_api.dll が実行ファイルと同じフォルダにあるか確認してください。\n\n" +
                    ex.Message,
                    "SteamVRエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }
            catch (BadImageFormatException ex)
            {
                logger.Error(ex, "OpenVR native DLL architecture mismatch");
                MessageBox.Show(
                    "OpenVR の DLL の形式が不正です(32/64bit不一致の可能性)。\n\n" +
                    ex.Message,
                    "SteamVRエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unexpected error during SteamVR init");
                MessageBox.Show(
                    "SteamVR 初期化中に予期しないエラーが発生しました。\n\n" +
                    ex,
                    "SteamVRエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }

            if(EVRErr != EVRInitError.None)
            {
                logger.Error("SteamVR init error: {0}", EVRErr);
                MessageBox.Show($"SteamVRでエラーが発生しています: {EVRErr}\nSteamVRを一度終了してから再度お試しください。", "SteamVRエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }

            OVRApp = OpenVR.Applications;
        }

        public static bool SetRegister(bool isEnable)
        {

            EVRApplicationError EVRError;

            if (isEnable)
            {
                EVRError = OVRApp.AddApplicationManifest(path, false);
            }
            else
            {
                EVRError = OVRApp.RemoveApplicationManifest(path);
            }

            bool isSuccess = EVRError == EVRApplicationError.None || EVRError == EVRApplicationError.UnknownApplication;

            if (!isSuccess) Debug.Print(EVRError.ToString());

            return isSuccess;
        }

        public static bool GetRegister()
        {
            return OVRApp.IsApplicationInstalled(pchkey);
        }

        public static bool SetAutoLaunch(bool isEnable)
        {
            EVRApplicationError EVRError = OVRApp.SetApplicationAutoLaunch(pchkey, isEnable);
            bool isSuccess = EVRError == EVRApplicationError.None || EVRError == EVRApplicationError.UnknownApplication;

            if (!isSuccess) Debug.Print(EVRError.ToString());

            return isSuccess;
        }

        public static bool GetAutoLaunch()
        {
            return OVRApp.GetApplicationAutoLaunch(pchkey);
        }
    }
}
