using Avalonia.Controls;
using Avalonia.ReactiveUI;
using AIRPG.ViewModels;

namespace AIRPG.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();
        }
    }
}

