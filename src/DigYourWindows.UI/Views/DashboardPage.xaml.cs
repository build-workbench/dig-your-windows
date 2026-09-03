using System.Windows.Controls;
using DigYourWindows.UI.ViewModels;

namespace DigYourWindows.UI.Views;

public partial class DashboardPage : Page
{
    public DashboardPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
