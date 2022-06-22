namespace Neolution.OrchardCoreModules.PageViewStats
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Localization;
    using Neolution.OrchardCoreModules.PageViewStats.Drivers;
    using OrchardCore.Navigation;

    public class PageViewStatsSettingsAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public PageViewStatsSettingsAdminMenu(IStringLocalizer<PageViewStatsSettingsAdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Page Views"], configuration => configuration
                    .Add(S["Settings"], S["Settings"].PrefixPosition(), settings => settings
                        .AddClass("pageViewStatsSettings").Id("pageViewStatsSettings")
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = PageViewStatsSiteSettingsDisplayDriver.GroupId })
                        .Permission(PageViewStatsPermissions.ManagePageViewStats)
                        .LocalNav())
                );
            }

            return Task.CompletedTask;
        }
    }
}
