using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace UKSF.Old.Launcher.UI.Dialog {
    public partial class DialogWindow {
        public enum DialogBoxType {
            OK,
            OK_CANCEL,
            YES_NO,
            YES_NO_CANCEL
        }

        private static DialogWindow dialog;
        private static bool open;
        private static MessageBoxResult result = MessageBoxResult.None;

        public DialogWindow() {
            InitializeComponent();

            AddHandler(Dialog.DialogTitleBarControl.DIALOG_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(DialogTitleBar_MouseDown));
            AddHandler(Dialog.DialogMainControl.DIALOG_MAIN_CONTROL_BUTTON_OK_CLICK_EVENT, new RoutedEventHandler(DialogButtonOK_Click));
            AddHandler(Dialog.DialogMainControl.DIALOG_MAIN_CONTROL_BUTTON_CANCEL_CLICK_EVENT, new RoutedEventHandler(DialogButtonCancel_Click));
            AddHandler(Dialog.DialogMainControl.DIALOG_MAIN_CONTROL_BUTTON_YES_CLICK_EVENT, new RoutedEventHandler(DialogButtonYes_Click));
            AddHandler(Dialog.DialogMainControl.DIALOG_MAIN_CONTROL_BUTTON_NO_CLICK_EVENT, new RoutedEventHandler(DialogButtonNo_Click));
        }

        public static MessageBoxResult Show(string title, string message, DialogBoxType type) {
            switch (type) {
                case DialogBoxType.OK: return Show(title, message, MessageBoxButton.OK);
                case DialogBoxType.OK_CANCEL: return Show(title, message, MessageBoxButton.OKCancel);
                case DialogBoxType.YES_NO: return Show(title, message, MessageBoxButton.YesNo);
                case DialogBoxType.YES_NO_CANCEL: return Show(title, message, MessageBoxButton.YesNoCancel);
                default: return Show(title, message, MessageBoxButton.OKCancel);
            }
        }

        public static MessageBoxResult Show(string title, string message, DialogBoxType type, params string[] buttonStrings) {
            switch (type) {
                case DialogBoxType.OK: return Show(title, message, MessageBoxButton.OK, null, buttonStrings);
                case DialogBoxType.OK_CANCEL: return Show(title, message, MessageBoxButton.OKCancel, null, buttonStrings);
                case DialogBoxType.YES_NO: return Show(title, message, MessageBoxButton.YesNo, null, buttonStrings);
                case DialogBoxType.YES_NO_CANCEL: return Show(title, message, MessageBoxButton.YesNoCancel, null, buttonStrings);
                default: return Show(title, message, MessageBoxButton.OKCancel, null, buttonStrings);
            }
        }

        public static MessageBoxResult Show(string title, string message, DialogBoxType type, UIElement control) {
            switch (type) {
                case DialogBoxType.OK: return Show(title, message, MessageBoxButton.OK, control);
                case DialogBoxType.OK_CANCEL: return Show(title, message, MessageBoxButton.OKCancel, control);
                case DialogBoxType.YES_NO: return Show(title, message, MessageBoxButton.YesNo, control);
                case DialogBoxType.YES_NO_CANCEL: return Show(title, message, MessageBoxButton.YesNoCancel, control);
                default: return Show(title, message, MessageBoxButton.OKCancel, control);
            }
        }

        private static MessageBoxResult Show(string title, string message, MessageBoxButton button, UIElement control = null, params string[] buttonStrings) {
            if (open) return MessageBoxResult.OK;
            open = true;
            dialog = new DialogWindow();
            dialog.DialogTitleBarControl.DialogTitleBarControlLabel.Content = title;
            dialog.DialogMainControl.DialogMainControlTextBlock.Text = message;

            if (control != null) {
                dialog.DialogMainControl.DialogMainControlGrid.Children.Add(control);
            }

            if (buttonStrings.Length > 0) {
                dialog.DialogMainControl.DialogMainControlButtonOk.Content = buttonStrings[0];
                dialog.DialogMainControl.DialogMainControlButtonYes.Content = buttonStrings[0];
                if (buttonStrings.Length > 1) {
                    dialog.DialogMainControl.DialogMainControlButtonCancel.Content = buttonStrings[1];
                    dialog.DialogMainControl.DialogMainControlButtonNo.Content = buttonStrings[1];
                }
            }

            SetButton(button);
            dialog.ShowDialog();
            open = false;
            return result;
        }

        private static void SetButton(MessageBoxButton button) {
            switch (button) {
                case MessageBoxButton.OK:
                    dialog.DialogMainControl.DialogMainControlButtonOk.Visibility = Visibility.Visible;
                    dialog.DialogMainControl.DialogMainControlButtonCancel.Visibility = Visibility.Collapsed;
                    dialog.DialogMainControl.DialogMainControlButtonYes.Visibility = Visibility.Collapsed;
                    dialog.DialogMainControl.DialogMainControlButtonNo.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    dialog.DialogMainControl.DialogMainControlButtonOk.Visibility = Visibility.Visible;
                    dialog.DialogMainControl.DialogMainControlButtonCancel.Visibility = Visibility.Visible;
                    dialog.DialogMainControl.DialogMainControlButtonYes.Visibility = Visibility.Collapsed;
                    dialog.DialogMainControl.DialogMainControlButtonNo.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo:
                    dialog.DialogMainControl.DialogMainControlButtonOk.Visibility = Visibility.Collapsed;
                    dialog.DialogMainControl.DialogMainControlButtonCancel.Visibility = Visibility.Collapsed;
                    dialog.DialogMainControl.DialogMainControlButtonYes.Visibility = Visibility.Visible;
                    dialog.DialogMainControl.DialogMainControlButtonNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    dialog.DialogMainControl.DialogMainControlButtonOk.Visibility = Visibility.Collapsed;
                    dialog.DialogMainControl.DialogMainControlButtonCancel.Visibility = Visibility.Visible;
                    dialog.DialogMainControl.DialogMainControlButtonYes.Visibility = Visibility.Visible;
                    dialog.DialogMainControl.DialogMainControlButtonNo.Visibility = Visibility.Visible;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        private void DialogTitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        private static void DialogButtonOK_Click(object sender, RoutedEventArgs args) {
            result = MessageBoxResult.OK;
            dialog.Close();
            dialog = null;
        }

        private static void DialogButtonCancel_Click(object sender, RoutedEventArgs args) {
            result = MessageBoxResult.Cancel;
            dialog.Close();
            dialog = null;
        }

        private static void DialogButtonYes_Click(object sender, RoutedEventArgs args) {
            result = MessageBoxResult.Yes;
            dialog.Close();
            dialog = null;
        }

        private static void DialogButtonNo_Click(object sender, RoutedEventArgs args) {
            result = MessageBoxResult.No;
            dialog.Close();
            dialog = null;
        }

        private static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs args) {
            Process.Start(args.Uri.ToString());
        }
    }
}
