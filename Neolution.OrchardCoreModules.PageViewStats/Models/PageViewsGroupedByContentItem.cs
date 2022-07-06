namespace Neolution.OrchardCoreModules.PageViewStats.Models;

public class PageViewsGroupedByContentItem
{
    public string ContentItemId { get; set; }
    public int TotalViews { get; set; }
    public int BotViews { get; set; }
    public int UniqueVisitors { get; set; }
}