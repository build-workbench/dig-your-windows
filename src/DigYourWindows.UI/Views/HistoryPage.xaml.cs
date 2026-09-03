using System.Windows.Controls;
using DigYourWindows.UI.ViewModels;

namespace DigYourWindows.UI.Views;

public partial class HistoryPage : Page
{
    public HistoryPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
