namespace Neolution.OrchardCoreModules.PageViewStats.ViewModels;

using System;

public class PageViewsPerDate
{
    public DateOnly Date { get; set; }
    public int Views { get; set; }
    public int BotViews { get; set; }
    public int UniqueVisitors { get; set; }
}