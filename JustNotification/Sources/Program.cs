using System;
using System.IO;
using System.Windows.Forms;
using NLog;

namespace JustNotification
{
    static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string appName = "JustNotification+";
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, appName);

            EnsureLogDirectory();
            SetupGlobalExceptionHandlers();

            bool hasHandle = false;
            try
            {
                try
                {
                    hasHandle = mutex.WaitOne(0, false);
                }
                catch (System.Threading.AbandonedMutexException)
                {
                    hasHandle = true;
                }
                if (hasHandle == false)
                {
                    // 多重起動判定

                    // SteamVRからの起動であればメッセージは表示しない
                    if(Array.IndexOf(Environment.GetCommandLineArgs(), "--steamvr") == -1)
                    {
                        MessageBox.Show("既に起動されているため、JustNotification+を起動できませんでした。", "起動エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    return;
                }

                // Initialize
                Init();

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                new MainWindow();
                Application.Run();
            }
            finally
            {
                if (hasHandle)
                {
                    mutex.ReleaseMutex();
                }
                mutex.Close();
            }

        }

        static void Init()
        {
            Notification.Init();
            SteamVR.Init();
            OverlayHandler.Init();
        }

        private static void EnsureLogDirectory()
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "logs"));
            }
            catch
            {
                // ログディレクトリ作成失敗は致命的ではないため握りつぶす
            }
        }

        private static void SetupGlobalExceptionHandlers()
        {
            try
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

                Application.ThreadException += (_, e) =>
                {
                    try
                    {
                        logger.Fatal(e.Exception, "Unhandled UI thread exception");
                        LogManager.Flush();
                    }
                    catch { }

                    try
                    {
                        MessageBox.Show(
                            "JustNotification+ が予期せず終了しました。\n\n" +
                            "詳細は logs フォルダのログを確認してください。\n\n" +
                            e.Exception,
                            "致命的なエラー",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    catch { }

                    Environment.Exit(1);
                };

                AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                {
                    try
                    {
                        if (e.ExceptionObject is Exception ex)
                        {
                            logger.Fatal(ex, "Unhandled AppDomain exception (IsTerminating={0})", e.IsTerminating);
                        }
                        else
                        {
                            logger.Fatal("Unhandled AppDomain exception (IsTerminating={0}): {1}", e.IsTerminating, e.ExceptionObject);
                        }

                        LogManager.Flush();
                    }
                    catch { }
                };

                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, e) =>
                {
                    try
                    {
                        logger.Error(e.Exception, "Unobserved task exception");
                        LogManager.Flush();
                    }
                    catch { }
                    e.SetObserved();
                };
            }
            catch
            {
                // ここで失敗しても起動自体は続行
            }
        }
    }
}
