using System.Windows;

namespace UKSF.Old.Launcher.UI.Main {
    public partial class MainWindow {
        public static MainWindow Instance;

        public MainWindow() {
            Instance = this;
            
            InitializeComponent();

            AddHandler(Main.TitleBarControl.MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(MainTitleBar_MouseDown));
            AddHandler(Main.TitleBarControl.MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT, new RoutedEventHandler(MainTitleBarButtonMinimize_Click));

            SettingsControl.Initialise();
        }

        public static void CreateMainWindow() {
            if (Instance == null) {
                Instance = new MainWindow();
            }
            
            Instance.Show();
            Instance.Activate();
            Instance.Focus();
        }

        private void MainTitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        private void MainTitleBarButtonMinimize_Click(object sender, RoutedEventArgs args) => WindowState = WindowState.Minimized;
    }
}
