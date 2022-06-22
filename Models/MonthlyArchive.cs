namespace Neolution.OrchardCoreModules.PageViewStats.Models;

using System.Collections.Generic;
using OrchardCore.Data.Documents;

public class MonthlyArchive : Document
{
    public int Month { get; set; }

    public int Year { get; set; }

    public ICollection<DailyArchive> Summaries { get; set; }
}