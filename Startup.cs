namespace Neolution.OrchardCoreModules.PageViewStats
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Neolution.OrchardCoreModules.PageViewStats.Drivers;
    using Neolution.OrchardCoreModules.PageViewStats.Models;
    using Neolution.OrchardCoreModules.PageViewStats.Services;
    using OrchardCore.BackgroundTasks;
    using OrchardCore.Data.Migration;
    using OrchardCore.DisplayManagement.Handlers;
    using OrchardCore.Modules;
    using OrchardCore.Navigation;
    using OrchardCore.Security.Permissions;
    using OrchardCore.Settings;

    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<ISite>, PageViewStatsSiteSettingsDisplayDriver>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, PageViewStatsSettingsAdminMenu>();
            services.AddScoped<INavigationProvider, PageViewStatsAdminMenu>();

            services.AddScoped<IDataMigration, Migrations>();
            
            services.AddSingleton<IBotDetector, BotDetector>();

            services.AddSingleton<IBackgroundTask, ArchivePageViewsBackgroundTask>();

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(PageViewStatsFilter));
            });
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "PageViewStats_CountPageView",
                areaName: "Neolution.OrchardCoreModules.PageViewStats",
                pattern: "PageViewStats/CountPageView",
                defaults: new { controller = "CountPageView", action = "Index" }
            );
        }
    }
}
