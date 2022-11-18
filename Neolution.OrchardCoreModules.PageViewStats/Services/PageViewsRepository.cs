namespace Neolution.OrchardCoreModules.PageViewStats.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Neolution.OrchardCoreModules.PageViewStats.Extensions;
using Neolution.OrchardCoreModules.PageViewStats.Models;
using Neolution.OrchardCoreModules.PageViewStats.Queries;
using OrchardCore.Documents;
using OrchardCore.Settings;

public interface IAggregateService
{
    Task<DailyArchive> CreateAggregateAsync(DateOnly date);
}

public class AggregateService : IAggregateService
{
    private readonly ISiteService siteService;
    private readonly ISender sender;
    private readonly IBotDetector botDetector;

    public AggregateService(ISiteService siteService, ISender sender, IBotDetector botDetector)
    {
        this.siteService = siteService;
        this.sender = sender;
        this.botDetector = botDetector;
    }

    public async Task<DailyArchive> CreateAggregateAsync(DateOnly date)
    {
        var siteSettings = await this.siteService.GetSiteSettingsAsync();

        var pageViews = await this.sender.Send(new GetPageViewsQuery(date));

        var aggregate = new DailyArchive
        {
            Id = date.ToString("yyyy-MM-dd"),
            DateTimeZoneId = siteSettings.TimeZoneId,
            CalculatedOn = DateTimeOffset.UtcNow,
            TotalViews = pageViews.Count,
            BotViews = pageViews.Count(x => this.botDetector.CheckUserAgentString(x.RequestUserAgentString)),
            UniqueVisitors = pageViews.Select(x => new { x.RequestIpAddress, x.RequestUserAgentString }).Distinct().Count(),
        };

        var groupedPageViews = await this.sender.Send(new GetPageViewsPerDayQuery(date));
        aggregate.PageViews = groupedPageViews
            .Select(x => new PageViewsGroupedByContentItem
            {
                ContentItemId = x.ContentItemId,
                TotalViews = x.TotalViews,
                BotViews = x.BotViews,
                UniqueVisitors = x.UniqueVisitors,
            }).ToList();

        return aggregate;
    }
}

public interface IPageViewsRepository
{
    Task<IList<DailyArchive>> LoadAllPageViewsAsync();
    Task<IList<DailyArchive>> LoadPageViewsAsync(DateOnly earliest, DateOnly latest);
}

public class PageViewsRepository : IPageViewsRepository
{
    private readonly ISiteService siteService;
    private readonly IAggregateService aggregateService;
    private readonly IDocumentManager<PageViewArchive> documentManager;

    public PageViewsRepository(ISiteService siteService, IAggregateService aggregateService, IDocumentManager<PageViewArchive> documentManager)
    {
        this.siteService = siteService;
        this.aggregateService = aggregateService;
        this.documentManager = documentManager;
    }

    public async Task<IList<DailyArchive>> LoadAllPageViewsAsync()
    {
        var pageViewArchive = await documentManager.GetOrCreateImmutableAsync();
        var result = pageViewArchive.DailyArchives;

        var settings = await this.siteService.GetSiteSettingsAsync();
        var today = DateOnlyExtensions.Today(settings);

        var isTodayIncluded = pageViewArchive.DailyArchives.Any(x => x.Id == today.ToString("yyyy-MM-dd"));
        if (!isTodayIncluded)
        {
            var todayViewArchive = await LoadPageViewsAsync(today.AddDays(-1), today).ConfigureAwait(false);
            if (todayViewArchive.Any())
            {
                result.Insert(0, todayViewArchive.First());
            }
        }

        return result;
    }

    public async Task<IList<DailyArchive>> LoadPageViewsAsync(DateOnly earliest, DateOnly latest)
    {
        var settings = await this.siteService.GetSiteSettingsAsync();
        var today = DateOnlyExtensions.Today(settings);

        var pageViewArchive = await documentManager.GetOrCreateMutableAsync();

        var isDirty = false;
        var result = new List<DailyArchive>();
        var amountOfDays = latest.DayNumber - earliest.DayNumber;
        for (int i = 0; i < amountOfDays; i++)
        {
            var date = latest.AddDays(i * -1);
            var dailyArchive = pageViewArchive.DailyArchives.FirstOrDefault(x => x.Id == date.ToString("yyyy-MM-dd"));
            if (dailyArchive == null)
            {
                dailyArchive = await this.aggregateService.CreateAggregateAsync(date);
                if (date != today)
                {
                    pageViewArchive.DailyArchives.Add(dailyArchive);
                    isDirty = true;
                }
            }

            result.Add(dailyArchive);
        }

        if (isDirty)
        {
            pageViewArchive.DailyArchives = pageViewArchive.DailyArchives.OrderByDescending(x => x.Id).ToList();
            await documentManager.UpdateAsync(pageViewArchive);
        }

        return result;
    }
}