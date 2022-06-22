namespace Neolution.OrchardCoreModules.PageViewStats.Models;

using System;

public class DailyArchive
{
    public DateOnly Date { get; set; }
    public int Views { get; set; }
    public int BotViews { get; set; }
    public int UniqueVisitors { get; set; }
}