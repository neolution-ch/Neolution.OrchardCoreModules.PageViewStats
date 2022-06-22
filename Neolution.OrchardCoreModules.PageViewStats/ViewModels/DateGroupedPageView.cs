namespace Neolution.OrchardCoreModules.PageViewStats.ViewModels;

using System;

public class DateGroupedPageView
{
    public DateTimeOffset Date { get; set; }
    public int Views { get; set; }
    public int BotViews { get; set; }
}