namespace Neolution.OrchardCoreModules.PageViewStats
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Neolution.OrchardCoreModules.PageViewStats.Models;
    using OrchardCore.Admin;
    using OrchardCore.Entities;
    using OrchardCore.ResourceManagement;
    using OrchardCore.Settings;

    public class PageViewStatsFilter : IAsyncResultFilter
    {
        private readonly ILogger<PageViewStatsFilter> logger;
        private readonly IResourceManager resourceManager;
        private readonly ISiteService siteService;
        private readonly IAntiforgery antiforgery;

        private HtmlString scriptsCache;

        public PageViewStatsFilter(ILogger<PageViewStatsFilter> logger, IResourceManager resourceManager, ISiteService siteService, IAntiforgery antiforgery)
        {
            this.logger = logger;
            this.resourceManager = resourceManager;
            this.siteService = siteService;
            this.antiforgery = antiforgery;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var settings = (await this.siteService.GetSiteSettingsAsync()).As<PageViewStatsSettings>();
            if (!settings.IsEnabled)
            {
                // Ignore filter when page view statistics are disabled
                await next();
                return;
            }

            // Should only run on the front-end for a full view
            if (AdminAttribute.IsApplied(context.HttpContext) || (context.Result is not ViewResult && context.Result is not PageResult))
            {
                await next();
                return;
            }

            if (scriptsCache == null)
            {
                try
                {
                    dynamic zoneHolding = context.Result switch
                    {
                        ViewResult viewResult => viewResult.Model,
                        PageResult pageResult => pageResult.Model,
                        _ => null
                    };

                    if (zoneHolding is null)
                    {
                        await next();
                        return;
                    }

                    var contentItem = zoneHolding.ContentItem;

                    // Do not count page view when content item is null
                    if (contentItem is null)
                    {
                        await next();
                        return;
                    }

                    var tokenSet = this.antiforgery.GetAndStoreTokens(context.HttpContext);

                    var scriptBuilder = new StringBuilder();
                    scriptBuilder.Append("<script>");
                    scriptBuilder.Append("document.addEventListener('DOMContentLoaded',function(){var xhr=new XMLHttpRequest();xhr.open('POST','/pageviewstats/countpageview',!0);xhr.setRequestHeader('Content-Type','application/x-www-form-urlencoded');");
                    scriptBuilder.AppendFormat("xhr.setRequestHeader('{0}','{1}');", tokenSet.HeaderName, tokenSet.RequestToken);
                    scriptBuilder.AppendFormat("var data='contentItemId={0}';", contentItem.ContentItemId);
                    scriptBuilder.Append("xhr.send(data)})");
                    scriptBuilder.AppendLine("</script>");

                    this.scriptsCache = new HtmlString(scriptBuilder.ToString());
                }
                catch (Exception ex)
                {
                    logger.LogError("Could not count page view", ex);
                }
            }

            if (scriptsCache != null)
            {
                this.resourceManager.RegisterFootScript(scriptsCache);
            }

            await next.Invoke();
        }
    }
}