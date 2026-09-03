using System.Windows.Controls;
using DigYourWindows.UI.ViewModels;

namespace DigYourWindows.UI.Views;

public partial class LogsPage : Page
{
    public LogsPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
