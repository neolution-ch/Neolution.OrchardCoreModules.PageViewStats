namespace Neolution.OrchardCoreModules.PageViewStats.ViewModels;

public class PageViewStatsSettingsViewModel
{
    public bool IsEnabled { get; set; }
    public bool CollectUserIp { get; set; }

    public bool CollectUserAgentString { get; set; }
}