using System.Windows.Controls;
using DigYourWindows.Core.Models;
using DigYourWindows.UI.ViewModels;

namespace DigYourWindows.UI.Views;

/// <summary>
/// Code-behind for HistoryListView.
/// </summary>
public partial class HistoryListView : UserControl
{
    public HistoryListView()
    {
        InitializeComponent();
        HistoryListBox.SelectionChanged += HistoryListBox_SelectionChanged;
    }

    private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not HistoryListViewModel viewModel)
        {
            return;
        }

        var entry = e.AddedItems.OfType<DiagnosticHistorySummary>().FirstOrDefault();
        if (entry is not null && viewModel.SelectEntryCommand.CanExecute(null))
        {
            viewModel.SelectEntryCommand.Execute(entry);
        }
    }
}
