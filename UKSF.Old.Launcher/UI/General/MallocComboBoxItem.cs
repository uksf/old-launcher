using System.Windows;
using System.Windows.Controls;
using UKSF.Old.Launcher.Game;

namespace UKSF.Old.Launcher.UI.General {
    public class MallocComboBoxItem : ComboBoxItem {
        public MallocComboBoxItem(MallocHandler.Malloc malloc, Style style) {
            Malloc = malloc;
            Content = Malloc.Name.Replace("_", "__");
            Style = style;
        }

        public MallocHandler.Malloc Malloc { get; }
    }
}
