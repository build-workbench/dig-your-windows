using DigYourWindows.Core.Services;
using ScottPlot.WPF;

namespace DigYourWindows.UI.Services;

/// <summary>
/// Owns the two ScottPlot controls and all rendering concerns (theme, series, axes).
/// ViewModels hand over data and receive ready-rendered plots; per ScottPlot's MVVM
/// guidance the WpfPlot instances are exposed for ContentControl binding.
/// </summary>
public interface IMonitorPlotService
{
    WpfPlot ReliabilityTrendPlot { get; }

    WpfPlot NetworkTrafficPlot { get; }

    /// <summary>Renders the reliability trend; an empty trend renders the empty-state axis.</summary>
    void RenderReliabilityTrend(IReadOnlyList<ReliabilityTrendDayCounts> trend, bool darkTheme);

    /// <summary>Renders the network traffic window; empty history renders the empty-state axis.</summary>
    void RenderNetworkTraffic(
        IReadOnlyList<DateTime> times,
        IReadOnlyList<double> download,
        IReadOnlyList<double> upload,
        bool darkTheme);
}

public sealed class MonitorPlotService : IMonitorPlotService
{
    public WpfPlot ReliabilityTrendPlot { get; } = new();

    public WpfPlot NetworkTrafficPlot { get; } = new();

    public void RenderReliabilityTrend(IReadOnlyList<ReliabilityTrendDayCounts> trend, bool darkTheme)
    {
        var plot = ReliabilityTrendPlot.Plot;
        plot.Clear();
        plot.Title("可靠性趋势");
        plot.XLabel("日期");
        plot.YLabel("记录数");

        if (trend.Count == 0)
        {
            ApplyTheme(plot, darkTheme);
            // 空数据时默认坐标轴 (-10..10) 看起来像渲染异常；占位提示由 XAML 覆盖层负责
            plot.Axes.SetLimits(0, 1, 0, 1);
            ReliabilityTrendPlot.Refresh();
            return;
        }

        var xs = trend.Select(d => d.Day.ToOADate()).ToArray();

        AddSeries(plot, xs, trend.Select(d => (double)d.Total).ToArray(), "总计", "#9E9E9E", lineWidth: 2);
        AddSeries(plot, xs, trend.Select(d => (double)d.ApplicationFailures).ToArray(), "应用程序故障", "#F44336");
        AddSeries(plot, xs, trend.Select(d => (double)d.WindowsFailures).ToArray(), "Windows 故障", "#FF9800");
        AddSeries(plot, xs, trend.Select(d => (double)d.OtherFailures).ToArray(), "其他故障", "#FFC107");
        AddSeries(plot, xs, trend.Select(d => (double)d.Unknown).ToArray(), "未知", "#9C27B0");

        plot.Legend.IsVisible = true;
        plot.Axes.DateTimeTicksBottom();
        ApplyTheme(plot, darkTheme);
        ReliabilityTrendPlot.Refresh();
    }

    public void RenderNetworkTraffic(
        IReadOnlyList<DateTime> times,
        IReadOnlyList<double> download,
        IReadOnlyList<double> upload,
        bool darkTheme)
    {
        var plot = NetworkTrafficPlot.Plot;
        plot.Clear();
        plot.Title("网络流量 (最近60秒)");
        plot.XLabel("时间");
        plot.YLabel("MB/s");

        ApplyTheme(plot, darkTheme);

        if (times.Count == 0)
        {
            NetworkTrafficPlot.Refresh();
            return;
        }

        var xs = times.Select(time => time.ToOADate()).ToArray();

        AddSeries(plot, xs, download.ToArray(), "下载", "#2196F3");
        AddSeries(plot, xs, upload.ToArray(), "上传", "#4CAF50");

        plot.Legend.IsVisible = true;
        plot.Axes.DateTimeTicksBottom();
        NetworkTrafficPlot.Refresh();
    }

    private static void AddSeries(
        ScottPlot.Plot plot,
        double[] xs,
        double[] ys,
        string legendText,
        string colorHex,
        float? lineWidth = null)
    {
        var scatter = plot.Add.Scatter(xs, ys);
        scatter.LegendText = legendText;
        scatter.Color = ScottPlot.Color.FromHex(colorHex);

        if (lineWidth.HasValue)
        {
            scatter.LineWidth = lineWidth.Value;
        }
    }

    private static void ApplyTheme(ScottPlot.Plot plot, bool darkTheme)
    {
        var backgroundColor = darkTheme ? ScottPlot.Color.FromHex("#1E1E1E") : ScottPlot.Color.FromHex("#FFFFFF");
        var textColor = darkTheme ? ScottPlot.Color.FromHex("#FFFFFF") : ScottPlot.Color.FromHex("#212529");
        var gridColor = darkTheme ? ScottPlot.Color.FromHex("#3E3E3E") : ScottPlot.Color.FromHex("#E0E0E0");

        plot.FigureBackground.Color = backgroundColor;
        plot.Axes.Color(textColor);
        plot.Grid.MajorLineColor = gridColor;
        // 标题/轴标签/图例含中文，默认字体渲染为方块，需自动选择支持的字体
        plot.Font.Automatic();

        foreach (var axis in plot.Axes.GetAxes())
        {
            axis.Label.ForeColor = textColor;
            axis.TickLabelStyle.ForeColor = textColor;
        }
    }
}
