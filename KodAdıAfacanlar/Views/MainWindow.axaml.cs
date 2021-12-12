using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KodAdıAfacanlar.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if (TIME || WORLDDEBUG)
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}