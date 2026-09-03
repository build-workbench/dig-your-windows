using System.Windows.Controls;
using DigYourWindows.UI.ViewModels;

namespace DigYourWindows.UI.Views;

public partial class MonitoringPage : Page
{
    public MonitoringPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
