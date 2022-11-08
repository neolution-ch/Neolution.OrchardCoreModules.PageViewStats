namespace Neolution.OrchardCoreModules.PageViewStats.UnitTests
{
    using Microsoft.Extensions.DependencyInjection;
    using Neolution.OrchardCoreModules.PageViewStats.Services;
    using Shouldly;

    public class BotDetectorTests
    {
        [Fact]
        public void GivenRobotUserAgents_WhenCheckingForRobot_ThenShouldIdentifiedAsRobots()
        {
            var botDetector = GetBotDetectorService();

            // Using real-world sample user agent strings that should be identified as robots
            botDetector.CheckUserAgentString("'DuckDuckBot-Https/1.1; (+https://duckduckgo.com/duckduckbot)'").ShouldBeTrue();
            botDetector.CheckUserAgentString("Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.115 Mobile Safari/537.36 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)").ShouldBeTrue();
            botDetector.CheckUserAgentString("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; Googlebot/2.1; +http://www.google.com/bot.html) Chrome/102.0.5005.115 Safari/537.36").ShouldBeTrue();
            botDetector.CheckUserAgentString("Mozilla/5.0 (compatible; AhrefsBot/7.0; +http://ahrefs.com/robot/)").ShouldBeTrue();
            botDetector.CheckUserAgentString("Mozilla/5.0 (compatible; ev-crawler/1.0; +https://headline.com/legal/crawler)").ShouldBeTrue();
        }

        [Fact]
        public void GivenHumanUserAgents_WhenCheckingForRobot_ThenShouldNotIdentifiedAsRobots()
        {
            var botDetector = GetBotDetectorService();

            // Using the top user agent strings from our websites database that look like human-operated browsers
            botDetector.CheckUserAgentString("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36").ShouldBeFalse();
            botDetector.CheckUserAgentString("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:101.0) Gecko/20100101 Firefox/101.0").ShouldBeFalse();
            botDetector.CheckUserAgentString("Mozilla/5.0 (iPhone; CPU iPhone OS 15_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.5 Mobile/15E148 Safari/604.1").ShouldBeFalse();
            botDetector.CheckUserAgentString("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36").ShouldBeFalse();
            botDetector.CheckUserAgentString("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.5 Safari/605.1.15").ShouldBeFalse();
            botDetector.CheckUserAgentString("Mozilla/5.0 (X11; Linux x86_64; rv:105.0) Gecko/20100101 Firefox/105.0").ShouldBeFalse();
        }

        private static IBotDetector GetBotDetectorService()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IBotDetector, BotDetector>();
            var serviceProvider = services.BuildServiceProvider();
            var botDetector = serviceProvider.GetRequiredService<IBotDetector>();
            return botDetector;
        }
    }
}