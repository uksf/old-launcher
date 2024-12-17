using System.Collections.Generic;
using System.Windows;
using UKSF.Launcher.Network;
using UKSF.Old.Launcher.UI.Main;

namespace UKSF.Old.Launcher.UI.General {
    public class SafeWindow : Window {
        protected SafeWindow() {
            if (Application.Current == null) {
                Core.ShutDown();
            }
        }

        public class StringRoutedEventArgs : RoutedEventArgs {
            public StringRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public string Text { get; set; }
        }

        public class BoolRoutedEventArgs : RoutedEventArgs {
            public BoolRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public bool State { get; set; }
        }

        public class IntRoutedEventArgs : RoutedEventArgs {
            public IntRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public int Value { get; set; }
        }

        public class ProgressRoutedEventArgs : RoutedEventArgs {
            public ProgressRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public string Message { get; set; }
            public int Value { get; set; }
        }

        public class WarningRoutedEventArgs : RoutedEventArgs {
            public WarningRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public bool Block { get; set; }
            public HomeControl.CurrentWarning CurrentWarning { get; set; }
            public string Warning { get; set; }
        }

        public class ServerRoutedEventArgs : RoutedEventArgs {
            public ServerRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public List<Server> Servers { get; set; }
        }
    }
}
