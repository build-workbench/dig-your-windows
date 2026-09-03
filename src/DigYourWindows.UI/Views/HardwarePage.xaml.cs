using System.Windows.Controls;
using DigYourWindows.UI.ViewModels;

namespace DigYourWindows.UI.Views;

public partial class HardwarePage : Page
{
    public HardwarePage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
