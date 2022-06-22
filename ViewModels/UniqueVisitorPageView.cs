namespace Neolution.OrchardCoreModules.PageViewStats.ViewModels;

using System;

public class UniqueVisitorPageView
{
    public string RequestIpAddress { get; set; }
    public string RequestUserAgentString { get; set; }
    public DateTimeOffset Date { get; set; }
}