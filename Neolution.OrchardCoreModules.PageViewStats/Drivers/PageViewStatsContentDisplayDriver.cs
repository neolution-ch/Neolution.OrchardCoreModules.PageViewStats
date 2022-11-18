namespace Neolution.OrchardCoreModules.PageViewStats.Drivers
{
    using OrchardCore.ContentManagement.Display.ContentDisplay;
    using OrchardCore.DisplayManagement.ModelBinding;
    using OrchardCore.DisplayManagement.Views;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using OrchardCore.ContentManagement;
    using OrchardCore.Settings;
    using Neolution.OrchardCoreModules.PageViewStats.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using OrchardCore.Admin;

    public class PageViewStatsContentDisplayDriver : ContentDisplayDriver
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAuthorizationService authorizationService;
        private readonly ISiteService siteService;
        private readonly IPageViewsRepository repository;

        public PageViewStatsContentDisplayDriver(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService, ISiteService siteService, IPageViewsRepository repository)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.authorizationService = authorizationService;
            this.siteService = siteService;
            this.repository = repository;
        }

        public override async Task<IDisplayResult> DisplayAsync(ContentItem contentItem, IUpdateModel updater)
        {
            var context = this.httpContextAccessor.HttpContext;

            // Should only render on the back-end
            if (!AdminAttribute.IsApplied(context))
            {
                return null;
            }
            
            // Should only render for users with proper permissions
            if (!await authorizationService.AuthorizeAsync(context?.User, PageViewStatsPermissions.ViewPageViewStats).ConfigureAwait(false))
            {
                return null;
            }

            var settings = await this.siteService.GetSiteSettingsAsync().ConfigureAwait(false);
            var tz = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var today = DateOnly.FromDateTime(now);

            var pageViews = await this.repository.LoadPageViewsAsync(today.AddDays(-30), today).ConfigureAwait(false);
            var contentPageViews = pageViews.SelectMany(x => x.PageViews).Where(x => x.ContentItemId == contentItem.ContentItemId).ToList();
            var totalViews = await this.repository.LoadAllPageViewsAsync().ConfigureAwait(false);
            var contentPageTotalViews = totalViews.SelectMany(x => x.PageViews).Where(x => x.ContentItemId == contentItem.ContentItemId).ToList();

            return Initialize<PageViewStatsContentDisplayViewModel>("PageViewStatsPart_SummaryAdmin", m =>
                {
                    m.DailyViews = contentPageViews.Select(x => x.TotalViews).FirstOrDefault();
                    m.MonthlyViews = contentPageViews.Sum(x => x.TotalViews);
                    m.TotalViews = contentPageTotalViews.Sum(x => x.TotalViews);
                })
                .Location("SummaryAdmin", "Meta:90");
        }
    }

    public class PageViewStatsContentDisplayViewModel
    {
        public int DailyViews { get; set; }
        public int MonthlyViews { get; set; }
        public int TotalViews { get; set; }
    }
}
