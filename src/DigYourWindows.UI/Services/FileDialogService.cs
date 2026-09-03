using System.Diagnostics;
using Microsoft.Win32;

namespace DigYourWindows.UI.Services;

/// <summary>
/// Abstracts file dialogs and shell reveal so ViewModels stay testable
/// and free of Win32/WPF dialog concerns.
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// Shows an open-file dialog; returns the selected path or null when cancelled.
    /// </summary>
    string? PickJsonFileToOpen();

    /// <summary>
    /// Opens the file with its associated application (best effort).
    /// </summary>
    void RevealFile(string filePath);
}

public sealed class FileDialogService : IFileDialogService
{
    public string? PickJsonFileToOpen()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            DefaultExt = ".json"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void RevealFile(string filePath)
    {
        try
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            // Opening is a convenience; the user can navigate to the file manually.
            // Common causes: file association missing, permission denied, or file locked.
            Debug.WriteLine($"Failed to open exported file: {ex.Message}");
        }
    }
}
