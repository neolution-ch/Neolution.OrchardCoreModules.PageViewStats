namespace Neolution.OrchardCoreModules.PageViewStats.Models;

using System.Collections.Generic;
using OrchardCore.Data.Documents;

public class PageViewArchive : Document
{
    public string TimeZoneId { get; set; }

    public IList<DailyArchive> DailyArchives { get; set; } = new List<DailyArchive>();
}