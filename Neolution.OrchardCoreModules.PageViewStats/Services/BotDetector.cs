namespace Neolution.OrchardCoreModules.PageViewStats.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Neolution.OrchardCoreModules.PageViewStats.Resources;

internal class BotDetector : IBotDetector
{
    private static IReadOnlyList<Regex> botList;

    public BotDetector()
    {
        // Read embedded robots list (https://github.com/atmire/COUNTER-Robots) from DLL and remove BOM (if present)
        var text = EmbeddedResources.ReadAllText("Neolution.OrchardCoreModules.PageViewStats.Resources>COUNTER_Robots_list.txt").Trim('\uFEFF', '\u200B');

        // Transform line by line into a list of regex expressions.
        var list = text.Split(new[] { "\r\n" }, StringSplitOptions.None).Select(x => x).Where(x => !string.IsNullOrWhiteSpace(x));
        botList = list.Select(bot => new Regex(bot, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToList();
    }

    public bool CheckUserAgentString(string userAgentString)
    {
        if (string.IsNullOrWhiteSpace(userAgentString))
        {
            return false;
        }

        return botList.Any(regex => regex.IsMatch(userAgentString));
    }
}