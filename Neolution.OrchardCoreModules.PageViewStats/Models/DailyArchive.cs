namespace Neolution.OrchardCoreModules.PageViewStats.Models;

using System;
using System.Collections.Generic;

public class DailyArchive
{
    public string Id { get; set; }
    public DateTimeOffset CalculatedOn { get; set; }
    public string DateTimeZoneId { get; set; }
    public int TotalViews { get; set; }
    public int BotViews { get; set; }
    public int UniqueVisitors { get; set; }

    public IList<PageViewsGroupedByContentItem> PageViews { get; set; }

}