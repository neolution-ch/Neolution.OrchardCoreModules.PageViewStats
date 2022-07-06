namespace Neolution.OrchardCoreModules.PageViewStats.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Neolution.OrchardCoreModules.PageViewStats.Extensions;
    using Neolution.OrchardCoreModules.PageViewStats.Models;
    using Neolution.OrchardCoreModules.PageViewStats.Queries;
    using Neolution.OrchardCoreModules.PageViewStats.ViewModels;
    using OrchardCore.Data;
    using OrchardCore.Documents;
    using OrchardCore.Environment.Shell;
    using OrchardCore.Settings;
    using YesSql;

    public class RequestPageViews
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public static RequestPageViews FromLastDays(int days)
        {
            return new RequestPageViews { From = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days * -1)) };
        }
    }

    public interface IAggregateService
    {
        Task<DailyArchive> CreateAggregateAsync(DateOnly date);
        Task<PageViewArchive> CreateAggregateAsync(int year, int month);
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

        public async Task<PageViewArchive> CreateAggregateAsync(int year, int month)
        {
            var settings = await this.siteService.GetSiteSettingsAsync();

            var model = new PageViewArchive
            {
                TimeZoneId = settings.TimeZoneId,
            };

            var lastDay = DateTime.DaysInMonth(year, month);
            for (int i = 1; i <= lastDay; i++)
            {
                var aggregate = await this.CreateAggregateAsync(new DateOnly(year, month, i));
                if (aggregate != null)
                {
                    model.DailyArchives.Add(aggregate);
                }
            }

            return model;
        }
    }

    public interface IPageViewsRepository
    {
        Task<IList<DailyArchive>> LoadPageViewsAsync(DateOnly earliest, DateOnly latest);
    }

    public class PageViewsRepository : IPageViewsRepository
    {
        private readonly ShellSettings shellSettings;
        private readonly ISiteService siteService;
        private readonly IStore store;
        private readonly IDbConnectionAccessor dbConnectionAccessor;
        private readonly ILogger<PageViewsRepository> logger;
        private readonly ISender sender;
        private readonly IAggregateService aggregateService;
        private readonly IDocumentManager<PageViewArchive> documentManager;
        private readonly string tablePrefix;

        public PageViewsRepository(ShellSettings shellSettings, ISiteService siteService, IStore store, IDbConnectionAccessor dbConnectionAccessor, ILogger<PageViewsRepository> logger, ISender sender, IAggregateService aggregateService, IDocumentManager<PageViewArchive> documentManager)
        {
            this.shellSettings = shellSettings;
            this.siteService = siteService;
            this.store = store;
            this.dbConnectionAccessor = dbConnectionAccessor;
            this.logger = logger;
            this.sender = sender;
            this.aggregateService = aggregateService;
            this.documentManager = documentManager;

            tablePrefix = shellSettings["TablePrefix"];

            if (!string.IsNullOrEmpty(tablePrefix))
            {
                tablePrefix += '_';
            }
        }

        public async Task<IList<DailyArchive>> LoadPageViewsAsync(DateOnly earliest, DateOnly latest)
        {
            var settings = await this.siteService.GetSiteSettingsAsync();
            var today = DateOnlyExtensions.Today(settings);

            var pageViewArchive = await documentManager.GetOrCreateMutableAsync();

            var documentIsDirty = false;
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
                        documentIsDirty = true;
                    }
                }

                result.Add(dailyArchive);
            }

            if (documentIsDirty)
            {
                pageViewArchive.DailyArchives = pageViewArchive.DailyArchives.OrderByDescending(x => x.Id).ToList();
                await documentManager.UpdateAsync(pageViewArchive);
            }

            return result;
        }

        /*
        public async DateGroupedPageView LoadPageViewsAsync(DateOnly date)
        {
            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();

            try
            {
                var dialect = store.Configuration.SqlDialect;
                var sqlBuilder = dialect.CreateBuilder(tablePrefix);

                sqlBuilder.Select();
                sqlBuilder.Table(PageView.TableName);
                sqlBuilder.Selector("*");
                sqlBuilder.WhereAnd($"{dialect.QuoteForColumnName("Id")} > @Id");
                sqlBuilder.OrderBy($"{dialect.QuoteForColumnName("Id")}");

                return await connection.QueryAsync<IndexingTask>(sqlBuilder.ToSqlString(), new { Id = afterTaskId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while loading page views");
                throw;
            }
        }

        public async DateGroupedPageView LoadPageViewsAsync(int year, int month)
        {
            
        }

        public async List<DateOnly> LoadAllDaysWithDataAsync()
        {
            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();

            try
            {
                var dialect = store.Configuration.SqlDialect;
                var sqlBuilder = dialect.CreateBuilder(tablePrefix);

                sqlBuilder.Select();
                sqlBuilder.Table(PageView.TableName);
                sqlBuilder.Selector("*");
                sqlBuilder.WhereAnd($"{dialect.QuoteForColumnName("Id")} > @Id");
                sqlBuilder.OrderBy($"{dialect.QuoteForColumnName("Id")}");

                return await connection.QueryAsync<IndexingTask>(sqlBuilder.ToSqlString(), new { Id = afterTaskId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while loading page views");
                throw;
            }
        }
        */
    }
}
