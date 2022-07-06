namespace Neolution.OrchardCoreModules.PageViewStats
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Neolution.OrchardCoreModules.PageViewStats.Controllers;
    using Neolution.OrchardCoreModules.PageViewStats.Drivers;
    using Neolution.OrchardCoreModules.PageViewStats.Models;
    using Neolution.OrchardCoreModules.PageViewStats.Navigation;
    using Neolution.OrchardCoreModules.PageViewStats.Services;
    using OrchardCore.Admin;
    using OrchardCore.BackgroundTasks;
    using OrchardCore.Data.Migration;
    using OrchardCore.DisplayManagement.Handlers;
    using OrchardCore.Modules;
    using OrchardCore.Navigation;
    using OrchardCore.Security.Permissions;
    using OrchardCore.Settings;
    using AdminMenu = Neolution.OrchardCoreModules.PageViewStats.Navigation.AdminMenu;
    using MediatR;


    public class Startup : StartupBase
    {
        private AdminOptions adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            this.adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<ISite>, PageViewStatsSettingsDisplayDriver>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, SettingsAdminMenu>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<IDataMigration, Migrations>();
            
            services.AddSingleton<IBotDetector, BotDetector>();
            services.AddScoped<IPageViewsRepository, PageViewsRepository>();
            services.AddScoped<IAggregateService, AggregateService>();

            services.AddMediatR(typeof(Startup));

            services.AddSingleton<IBackgroundTask, ArchivePageViewsBackgroundTask>();

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(PageViewStatsFilter));
            });
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Frontend routes
            routes.MapAreaControllerRoute(
                name: "PageViewStats_CountPageView",
                areaName: "Neolution.OrchardCoreModules.PageViewStats",
                pattern: "PageViewStats/CountPageView",
                defaults: new { controller = "CountPageView", action = "Index" }
            );

            // Admin routes
            routes.MapAreaControllerRoute(
                name: "PageViewStats_DashboardIndex",
                areaName: "Neolution.OrchardCoreModules.PageViewStats",
                pattern: this.adminOptions.AdminUrlPrefix + "/PageViewStats/Dashboard",
                defaults: new { controller = "Dashboard", action = nameof(DashboardController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "PageViewStats_DashboardIndex",
                areaName: "Neolution.OrchardCoreModules.PageViewStats",
                pattern: this.adminOptions.AdminUrlPrefix + "/PageViewStats/Day/{day:datetime}", /* /Admin/PageViewStats/Day/2022-07-04 */
                defaults: new { controller = "Day", action = nameof(DayController.Index) }
            );
        }
    }
}
