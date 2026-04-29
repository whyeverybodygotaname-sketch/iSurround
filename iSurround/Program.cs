using iSurround.Models;
using iSurround.UIForms;
using iSurroundShared;
using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iSurround
{
    public static class Program
    {
        //internal static string SharedConfig.AppDataPath;
        //public static string AppProfilePath = Path.Combine(AppDataPath, "Profiles");
        private static readonly string AppProfileStoragePath = SharedConfig.ProfilesPath;
        public static string AppVersion = ThisAssembly.AssemblyFileVersion;
        public static bool IsCapturingHotkey = false;
        public static ProgramSettings AppProgramSettings;
        public static MainForm AppMainForm;

        public static CancellationTokenSource AppCancellationTokenSource = new CancellationTokenSource();
        public static SemaphoreSlim AppBackgroundTaskSemaphoreSlim = new SemaphoreSlim(1, 1);
        private static HashSet<string> registeredHotkeyUUIDs = new HashSet<string>();
        private const string MutexName = "iSurround_SingleInstance_Mutex";

        [STAThread]
        private static int Main(string[] args)
        {
            // 计算数据根目录（我的文档\iSurround）
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDataPath = Path.Combine(myDocs, "iSurround");
            iSurroundShared.SharedConfig.Initialize(appDataPath);

            // 创建必要目录
            Directory.CreateDirectory(iSurroundShared.SharedConfig.AppDataPath);
            Directory.CreateDirectory(iSurroundShared.SharedConfig.ProfilesPath);

            // 单实例互斥锁检测
            using (Mutex mutex = new Mutex(true, MutexName, out bool createdNew))
            {
                if (!createdNew)
                {
                    // 已有实例运行，当前实例退出
                    Console.WriteLine("Another instance is already running.");
                    return 1;
                }

                // 加载设置
                AppProgramSettings = ProgramSettings.LoadSettings();

                // 创建必要目录
                //foreach (string dir in new[] { AppProfilePath })
                //{
                //    if (!Directory.Exists(dir))
                //    {
                //        try { Directory.CreateDirectory(dir); }
                //        catch (Exception ex) { Console.WriteLine($"Cannot create directory {dir}: {ex.Message}"); }
                //    }
                //}

              
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // 启动主窗体
                AppMainForm = new MainForm();
                RefreshHotkeys();   // 替换 RegisterAllHotkeys()
                Application.Run(AppMainForm);
            }

            return 0;
        }

        

        // 当配置文件发生变动时，需要调用这个方法重新刷新热键
        public static void RefreshHotkeys()
        {
            // 1. 移除所有已注册的热键
            foreach (var uuid in registeredHotkeyUUIDs)
            {
                try
                {
                    HotkeyManager.Current.Remove(uuid);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"移除热键 {uuid} 失败: {ex.Message}");
                }
            }
            registeredHotkeyUUIDs.Clear();

            // 2. 重新注册所有配置文件中的热键
            foreach (var profile in ProfileRepository.AllProfiles)
            {
                if (profile.Hotkey != Keys.None)
                {
                    try
                    {
                        var localProfile = profile;
                        // 使用 AddOrReplace 是安全的
                        HotkeyManager.Current.AddOrReplace(localProfile.UUID, localProfile.Hotkey, (sender, e) =>
                        {
                            if (IsCapturingHotkey) return; // 如果还在捕获热键，忽略切换
                            var p = ProfileRepository.GetProfile(localProfile.UUID);
                            if (p != null)
                                ApplyProfileTask(p);
                        });
                        registeredHotkeyUUIDs.Add(localProfile.UUID);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"为配置 '{profile.Name}' 注册热键失败: {ex.Message}");
                    }
                }
            }
        }




        public static ApplyProfileResult ApplyProfileTask(ProfileItem profile)
        {
            if (AppBackgroundTaskSemaphoreSlim.CurrentCount == 0)
                return ApplyProfileResult.Error;

            bool gotLock = AppBackgroundTaskSemaphoreSlim.Wait(0);
            if (!gotLock)
                return ApplyProfileResult.Error;

            if (AppCancellationTokenSource != null)
                AppCancellationTokenSource.Dispose();
            AppCancellationTokenSource = new CancellationTokenSource();

            ApplyProfileResult result = ApplyProfileResult.Error;
            try
            {
                var task = Task.Run(() => ProfileRepository.ApplyProfile(profile));
                task.Wait(120);
                result = task.Result;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("ApplyProfile cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApplyProfileTask error: {ex.Message}");
            }
            finally
            {
                if (gotLock) AppBackgroundTaskSemaphoreSlim.Release();
            }

            if (result == ApplyProfileResult.Successful && AppMainForm != null)
            {
                var update = new System.Windows.Forms.MethodInvoker(() => AppMainForm.UpdateNotifyIconText($"iSurround ({profile.Name})"));
                if (AppMainForm.InvokeRequired)
                    AppMainForm.BeginInvoke(update);
                else
                    update();
            }
            return result;
        }

        public static bool IsValidFilename(string testName)
        {
            if (string.IsNullOrEmpty(testName)) return false;
            return testName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
        }
    }
}