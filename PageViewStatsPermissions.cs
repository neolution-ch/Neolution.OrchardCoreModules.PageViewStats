namespace Neolution.OrchardCoreModules.PageViewStats;

using OrchardCore.Security.Permissions;

public class PageViewStatsPermissions
{
    public static readonly Permission ViewPageViewStats = new Permission(nameof(ViewPageViewStats), "View page view statistics");
    public static readonly Permission ManagePageViewStats = new Permission(nameof(ManagePageViewStats), "Manage page view statistics settings");
}