namespace Neolution.OrchardCoreModules.PageViewStats.Extensions;

using System;
using OrchardCore.Settings;

internal static class DateOnlyExtensions
{
    internal static DateOnly Today(ISite settings)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        return DateOnly.FromDateTime(now);
    }
}