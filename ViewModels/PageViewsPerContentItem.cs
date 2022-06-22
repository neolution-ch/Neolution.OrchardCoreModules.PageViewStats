namespace Neolution.OrchardCoreModules.PageViewStats.ViewModels;

using OrchardCore.Autoroute.Models;

public class PageViewsPerContentItem
{
    public string ContentItemId { get; set; }
    public int Amount { get; set; }
    public string DisplayText { get; set; }
    public AutoroutePart Route { get; set; }
}