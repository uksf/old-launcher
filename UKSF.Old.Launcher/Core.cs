using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UKSF.Launcher.Network;
using UKSF.Old.Launcher.Game;
using UKSF.Old.Launcher.UI.Dialog;
using UKSF.Old.Launcher.UI.FTS;
using UKSF.Old.Launcher.UI.General;
using UKSF.Old.Launcher.UI.Login;
using UKSF.Old.Launcher.UI.Main;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher {
    public class Core {
        public static CancellationTokenSource CancellationTokenSource;
        public static SettingsHandler SettingsHandler;

        public static async void Start(bool updated) {
#if DEBUG
            Global.Debug = true;
#endif
            await StartAsync(updated).ConfigureAwait(false);
        }

        private static async Task StartAsync(bool updated) {
            try {
                CancellationTokenSource = new CancellationTokenSource();

                LogHandler.StartLogging();
                LogHandler.LogSpacerMessage(Global.Severity.INFO, "Launcher Started");

                if (updated) {
                    File.Delete(Path.Combine(Environment.CurrentDirectory, "UKSF.Launcher.Updater.exe"));
                }

                InitialiseSettings();
//                await Login();
                await FinishInit();
            } catch (Exception exception) {
                Error(exception);
            }
        }

        private static async Task Login() {
            LogHandler.LogSpacerMessage(Global.Severity.INFO, "Logging in");
            string message = "";

            if (!string.IsNullOrEmpty(Global.Settings.LoginEmail)) {
                string storedPassword = SettingsHandler.ParseSetting(nameof(Global.Settings.LoginPassword), "", true);
                if (!string.IsNullOrEmpty(storedPassword)) {
                    string decryptedPassword = PasswordHandler.DecryptPassword(storedPassword);
                    try {
                        await ApiWrapper.Login(Global.Settings.LoginEmail, decryptedPassword);
                        LogHandler.LogInfo("Logged in");
                        await Application.Current.Dispatcher.InvokeAsync(LoginWindow.CloseWindow);
                        await FinishInit();
                        return;
                    } catch (Exception exception) {
                        if (exception is LoginFailedException loginFailedException) {
                            LogHandler.LogSeverity(Global.Severity.ERROR, $"Failed to login because: {loginFailedException.Reason}");
                            message = loginFailedException.Reason;
                        } else {
                            LogHandler.LogSeverity(Global.Severity.ERROR, "Failed to login for an unknown reason");
                        }
                    }
                } else {
                    LogHandler.LogInfo("No stored password found");
                }
            } else {
                LogHandler.LogInfo("No stored email found");
            }

            await Application.Current.Dispatcher.InvokeAsync(() => {
                LoginWindow.CreateLoginWindow(async tuple => await LoginEvent(tuple));
                LoginWindow.UpdateDetails(message, Global.Settings.LoginEmail);
            });
        }

        private static async Task<string> LoginEvent(Tuple<MessageBoxResult, string, string> result) {
            (MessageBoxResult messageBoxResult, string email, string password) = result;
            if (messageBoxResult == MessageBoxResult.Cancel) {
                ShutDown();
                return null;
            }

            Global.Settings.LoginEmail = (string) SettingsHandler.WriteSetting(nameof(Global.Settings.LoginEmail), email);
            SettingsHandler.WriteSetting(nameof(Global.Settings.LoginPassword), PasswordHandler.EncryptPassword(password), true);
            await Login();

            return null;
        }

        private static async Task FinishInit() {
            try {
                await UpdateHandler.Initialise();
                await AccountHandler.Initialise();
                await ProfileHandler.Initialise();

                await Application.Current.Dispatcher.InvokeAsync(() => {
                    if (!Global.Settings.FirstTimeSetupDone) {
                        LogHandler.LogSpacerMessage(Global.Severity.INFO, "Running first time setup");
                        new FtsWindow().ShowDialog();
                    }

                    MainWindow.CreateMainWindow();
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = true});
                });

                //                BackgroundWorker repoBackgroundWorker = new BackgroundWorker {WorkerReportsProgress = true};
                //                repoBackgroundWorker.DoWork += (sender, args) => ServerHandler.StartServerHandler();
                //                repoBackgroundWorker.ProgressChanged += (sender, args) =>
                //                    UKSF.Launcher.UI.Main.MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(HomeControl.HOME_CONTROL_PROGRESS_EVENT) {
                //                        Value = args.ProgressPercentage, Message = args.UserState.ToString()
                //                    });
                //
                //                Global.Repo = new RepoClient(Global.Settings.ModLocation, Global.Constants.APPDATA, "uksf", LogHandler.LogInfo, repoBackgroundWorker.ReportProgress);
                //                Global.Repo.ErrorEvent += (sender, exception) => Error(exception);
                //                Global.Repo.ErrorNoShutdownEvent += (sender, exception) => ErrorNoShutdown(exception);
                //                Global.Repo.UploadEvent += (sender, requestTuple) => ServerHandler.SendDeltaRequest(requestTuple.Item1, requestTuple.Item2, requestTuple.Item3, requestTuple.Item4);
                //                Global.Repo.DeleteEvent += (sender, path) => ServerHandler.SendDeltaDelete(path);
                //                repoBackgroundWorker.RunWorkerAsync();
            } catch (Exception exception) {
                Error(exception);
            }
        }

        private static void InitialiseSettings() {
            LogHandler.LogSpacerMessage(Global.Severity.INFO, "Reading all settings");
            SettingsHandler = new SettingsHandler(Global.Constants.REGSITRY);

            // Launcher
            Global.Settings.FirstTimeSetupDone = SettingsHandler.ParseSetting(nameof(Global.Settings.FirstTimeSetupDone), false);
            Global.Settings.AutoUpdateLauncher = SettingsHandler.ParseSetting(nameof(Global.Settings.AutoUpdateLauncher), true);
            Global.Settings.LoginEmail = SettingsHandler.ParseSetting(nameof(Global.Settings.LoginEmail), "");

            // Game
            Global.Settings.GameLocation = SettingsHandler.ParseSetting(nameof(Global.Settings.GameLocation), "");
            Global.Settings.ModLocation = SettingsHandler.ParseSetting(nameof(Global.Settings.ModLocation), "");
            Global.Settings.Profile = SettingsHandler.ParseSetting(nameof(Global.Settings.Profile), "");

            // Startup
            Global.Settings.StartupNoSplash = SettingsHandler.ParseSetting(nameof(Global.Settings.StartupNoSplash), true);
            Global.Settings.StartupScriptErrors = SettingsHandler.ParseSetting(nameof(Global.Settings.StartupScriptErrors), false);
            Global.Settings.StartupHugePages = SettingsHandler.ParseSetting(nameof(Global.Settings.StartupHugePages), false);
            Global.Settings.StartupMalloc = SettingsHandler.ParseSetting(nameof(Global.Settings.StartupMalloc), Global.Constants.MALLOC_SYSTEM_DEFAULT);
            Global.Settings.StartupFilePatching = SettingsHandler.ParseSetting(nameof(Global.Settings.StartupFilePatching), false);
        }

        public static void ShutDown() {
            CancellationTokenSource.Cancel();
            //ServerHandler.Stop();
            if (Application.Current == null) {
                Environment.Exit(0);
            } else {
                Application.Current.Shutdown();
            }
        }

        public static async void Error(Exception exception) {
            await Application.Current.Dispatcher.InvokeAsync(async () => {
                CancellationTokenSource.Cancel();
                await Task.Delay(250);
                if (MainWindow.Instance != null) {
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(HomeControl.HOME_CONTROL_PROGRESS_EVENT) {Value = 1, Message = "stop"});
                }

                string message = await LogError(exception);
                MessageBoxResult result = DialogWindow.Show("Error",
                                                            $"Something went wrong.\nIf you wish to report the issue, press 'Send Report'\n\n{message}",
                                                            DialogWindow.DialogBoxType.OK_CANCEL,
                                                            "Send Report");
                if (result == MessageBoxResult.OK) { }

                ShutDown();
            });
        }

        private static async void ErrorNoShutdown(Exception exception) {
            await Application.Current.Dispatcher.InvokeAsync(async () => {
                string message = await LogError(exception);
                MessageBoxResult result = DialogWindow.Show("Error",
                                                            $"Something went wrong.\nIf you wish to report the issue, press 'Send Report'\n\n{message}",
                                                            DialogWindow.DialogBoxType.OK_CANCEL,
                                                            "Send Report");
                if (result == MessageBoxResult.OK) { }
            });
        }

        private static async Task<string> LogError(Exception exception) {
            string error = $"{exception.Message}\n{exception.StackTrace}";
            if (exception.InnerException != null) {
                error += $"\n\n Inner Exception: {exception.InnerException.Message}\n{exception.InnerException.StackTrace}";
            }

            string message = $"{exception.Message}\n{error.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None).Take(3)}";
            LogHandler.LogSeverity(Global.Severity.ERROR, error);
            await SendReport(error);
            return message;
        }

        private static async Task SendReport(string message) {
            if (string.IsNullOrEmpty(ApiWrapper.Token)) return;
            try {
                await ApiWrapper.Post("launcher/error", new {version = Global.VERSION.ToString(), message});
            } catch (Exception exception) {
                LogHandler.LogSeverity(Global.Severity.ERROR, exception.ToString());
            }
        }
    }
}
