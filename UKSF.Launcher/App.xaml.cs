using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using UKSF.Launcher.Services;

namespace UKSF.Launcher {
    public partial class App : Application {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr window);
        
        public App() {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs args) {
            using (new Mutex(true, "UKSF Launcher", out bool newInstance)) {
                if (newInstance) {
                    bool updated = false;
                    for (int i = 0; i != args.Args.Length; ++i) {
                        if (args.Args[i] == "-u") {
                            updated = true;
                        }
                    }

                    Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    DispatcherHelper.Initialize();
                    //ServiceLocator.Current.GetInstance<IStartupService>().Start(updated);
                } else {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName).Where(process => process.Id != current.Id)) {
                        SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }
            }

            base.OnStartup(args);
        }

        protected override void OnExit(ExitEventArgs e) {
//            LogHandler.LogSpacerMessage(Global.Severity.INFO, "Launcher Stopped");
//            LogHandler.CloseLog();
            base.OnExit(e);
        }
    }
}
