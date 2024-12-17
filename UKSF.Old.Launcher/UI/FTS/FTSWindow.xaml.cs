using System.Windows;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher.UI.FTS {
    public partial class FtsWindow {
        public FtsWindow() {
            InitializeComponent();

            AddHandler(FTS.FtsTitleBarControl.FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(FTS_TitleBar_MouseDown));
            AddHandler(FTS.FtsMainControl.FTS_MAIN_CONTROL_FINISH_EVENT, new RoutedEventHandler(FTS_Finish));
        }

        private void FTS_TitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        private void FTS_Finish(object sender, RoutedEventArgs args) {
            LogHandler.LogInfo("First time setup finished");
            Global.Settings.FirstTimeSetupDone = (bool) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.FirstTimeSetupDone), true);
            Close();
        }
    }
}
