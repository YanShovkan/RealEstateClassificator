using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace RealEstateClassificator.Core.Services;

public static class WebDriver
{
    public static IWebDriver SetupWebDriver(ChromeOptions chromeOptions)
    {
        var driver = new ChromeDriver(Environment.CurrentDirectory, chromeOptions);

        driver.ExecuteCdpCommand("Network.setBlockedURLs", new Dictionary<string, object>
            {
                {
                    "urls", new[]
                    {
                        "yastatic.net", "googletagmanager.com", "mc.yandex.ru", "buzzoola.com", "yandex.ru",
                        "hcaptcha.com", "avito.st", "content-autofill.googleapis.com", "uxfeedback.ru",
                        "optimizationguide-pa.googleapis.com", "yastatic.net", "google.com", "gstatic.com",
                        "fonts.googleapis.com", "top-fwz1.mail.ru", "tag.rutarget.ru", "*.css",
                        "www.googletagmanager.com", "tube.buzzoola.com", "content.adriver.ru",
                        "top-fwz1.mail.ru", "cs.avito.ru", "code.i8y156.ru", "st.hybrid.ai"
                    }
                }
            });

        driver.ExecuteCdpCommand("Network.enable", new Dictionary<string, object> { });
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
        return driver;
    }
}