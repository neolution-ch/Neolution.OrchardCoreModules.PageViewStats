namespace Neolution.OrchardCoreModules.PageViewStats.ViewModels;

using System;
using System.Collections.Generic;

public class DayIndexViewModel
{
    public DateOnly Date { get; set; }
    public List<PageViewsPerContentItem> Items { get; set; }
}