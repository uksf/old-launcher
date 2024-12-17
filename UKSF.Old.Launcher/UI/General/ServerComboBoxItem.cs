using System.Windows;
using System.Windows.Controls;
using UKSF.Launcher.Network;

namespace UKSF.Old.Launcher.UI.General {
    public class ServerComboBoxItem : ComboBoxItem {
        public ServerComboBoxItem(Server server, Style style) {
            Server = server;
            Style = style;
            Content = Server.Name;
        }

        public Server Server { get; }
    }
}
