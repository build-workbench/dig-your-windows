using System;
using DigYourWindows.Core.Services;
using Wpf.Ui.Abstractions;

namespace DigYourWindows.UI.Services;

/// <summary>
/// Service provider adapter for WPF-UI NavigationView navigation.
/// </summary>
public sealed class PageService : INavigationViewPageProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogService _log;

    public PageService(IServiceProvider serviceProvider, ILogService log)
    {
        _serviceProvider = serviceProvider;
        _log = log;
    }

    public object? GetPage(Type pageType)
    {
        _log.Info($"PageService.GetPage requested for {pageType.FullName}");
        try
        {
            var page = _serviceProvider.GetService(pageType);
            _log.Info($"PageService.GetPage resolved: {page != null}");
            return page;
        }
        catch (Exception ex)
        {
            _log.LogError($"PageService.GetPage failed for {pageType.FullName}: {ex.Message}", ex);
            throw;
        }
    }
}