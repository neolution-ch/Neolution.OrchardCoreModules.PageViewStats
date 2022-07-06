namespace Neolution.OrchardCoreModules.PageViewStats.Models;

using System;
using OrchardCore.Settings;

internal class UtcDay
{
    public DateTimeOffset From { get; private init; }
    public DateTimeOffset Until { get; private init; }

    internal static UtcDay Create(DateOnly date, string timeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        var localStart = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        var localEnd = localStart.AddDays(1).AddTicks(-1);

        var utcStart = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localStart, tz));
        var utcEnd = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localEnd, tz));

        return new UtcDay { From = utcStart, Until = utcEnd, };
    }
}