namespace Neolution.OrchardCoreModules.PageViewStats.Queries;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using OrchardCore.Data;
using OrchardCore.Settings;
using YesSql;

public class GetAvailablePageViewDatesQuery : IRequest<IList<DateOnly>>
{
}

public class GetAvailablePageViewDatesQueryHandler : IRequestHandler<GetAvailablePageViewDatesQuery, IList<DateOnly>>
{
    private readonly ISiteService siteService;
    private readonly IDbConnectionAccessor dbConnectionAccessor;

    public GetAvailablePageViewDatesQueryHandler(ISiteService siteService, IDbConnectionAccessor dbConnectionAccessor)
    {
        this.siteService = siteService;
        this.dbConnectionAccessor = dbConnectionAccessor;
    }

    public async Task<IList<DateOnly>> Handle(GetAvailablePageViewDatesQuery request, CancellationToken cancellationToken)
    {
        var settings = await this.siteService.GetSiteSettingsAsync();

        await using var connection = this.dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        /* LINQ2SQL:

         PageViews
            .OrderByDescending(x => x.CreatedUtc)
            .Select(x => new { x.CreatedUtc });

        */

        const string query = "SELECT [CreatedUtc] FROM [PageViews] ORDER BY [CreatedUtc] DESC";
        var pageViewDates = (await connection.QueryAsync<DateTimeOffset>(query)).ToList();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
        return pageViewDates.Select(x => TimeZoneInfo.ConvertTimeFromUtc(x.UtcDateTime, tz)).Select(DateOnly.FromDateTime).Distinct().ToList();
    }
}