namespace Neolution.OrchardCoreModules.PageViewStats;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

public class PageViewStatsAdminMenu : INavigationProvider
{
    private readonly IStringLocalizer S;

    public PageViewStatsAdminMenu(IStringLocalizer<PageViewStatsAdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            builder.Add(S["Page Views"], S["Page Views"].PrefixPosition(), pageViewStats => pageViewStats
                .AddClass("pageViewStats").Id("pageViewStats")
                .Add(S["Statistics"], S["Statistics"].PrefixPosition(), settings => settings
                    .Action("Index", "PageViewStats", new { area = "Neolution.OrchardCoreModules.PageViewStats" })
                    .Permission(PageViewStatsPermissions.ViewPageViewStats)
                    .LocalNav()
                )
            );
        }
        return Task.CompletedTask;
    }
}