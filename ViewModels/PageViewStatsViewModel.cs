namespace Neolution.OrchardCoreModules.PageViewStats.ViewModels;

using System.Collections.Generic;

public class PageViewStatsViewModel
{
    public IList<PageViewsPerContentItem> GroupedPageViews { get; set; }
    public IList<PageViewsPerDate> DateGroupedPageViews { get; set; }
    public int History { get; set; }
}