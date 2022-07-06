namespace Neolution.OrchardCoreModules.PageViewStats.Navigation;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

public class AdminMenu : INavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            builder.Add(S["Page Views"], S["Page Views"].PrefixPosition(), pageViewStats => pageViewStats
                .AddClass("pageViewStats").Id("pageViewStats")
                .Add(S["Dashboard"], S["Dashboard"].PrefixPosition("1"), settings => settings
                    .Action("Index", "Dashboard", new { area = "Neolution.OrchardCoreModules.PageViewStats" })
                    .Permission(PageViewStatsPermissions.ViewPageViewStats)
                    .LocalNav()
                )
            );
        }
        return Task.CompletedTask;
    }
}