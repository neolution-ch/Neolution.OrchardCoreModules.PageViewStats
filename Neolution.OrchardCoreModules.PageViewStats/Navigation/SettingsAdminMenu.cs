namespace Neolution.OrchardCoreModules.PageViewStats.Navigation;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Neolution.OrchardCoreModules.PageViewStats.Drivers;
using OrchardCore.Navigation;

public class SettingsAdminMenu : INavigationProvider
{
    private readonly IStringLocalizer S;

    public SettingsAdminMenu(IStringLocalizer<SettingsAdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            builder.Add(S["Page Views"], configuration => configuration
                .Add(S["Settings"], S["Settings"].PrefixPosition("2"), settings => settings
                    .AddClass("pageViewStatsSettings").Id("pageViewStatsSettings")
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = PageViewStatsSettingsDisplayDriver.GroupId })
                    .Permission(PageViewStatsPermissions.ManagePageViewStats)
                    .LocalNav())
            );
        }

        return Task.CompletedTask;
    }
}