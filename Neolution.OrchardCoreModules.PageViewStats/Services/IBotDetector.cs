namespace Neolution.OrchardCoreModules.PageViewStats.Services;

public interface IBotDetector
{
    bool CheckUserAgentString(string userAgentString);
}