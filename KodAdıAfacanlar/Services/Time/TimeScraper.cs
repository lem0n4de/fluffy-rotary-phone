using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Cookie = System.Net.Cookie;

namespace KodAdıAfacanlar.Services.Time
{
    public class TimeScraper
    {
        private const string LoginPage =
            "http://88.255.86.4:85/Account/Login?ReturnUrl=%2f%3fPrivateAppKey%3d16cdab2b-aa4f-4da4-ae76-0c416502fc05&PrivateAppKey=16cdab2b-aa4f-4da4-ae76-0c416502fc05";

        private const string Username = "eslemaksu01@gmail.com"; // "yunusey97@gmail.com";
        private const string Password = "942658584"; // "205ynsmr";
        private bool loggedIn = false;
        private Dictionary<string, string> cookies = new();
        private const string SessionCookie = "ASP.NET_SessionId";
        private const string AuthCookie = ".ASPXAUTH";
        private string BreezeCookie = "";

        private async Task TestWebdriverProxy(WebDriver driver)
        {
            driver.Navigate().GoToUrl("https://icanhazip.com");
            var ip = driver.FindElement(By.TagName("pre")).Text;
            using (var client = new HttpClient())
            {
                var proxylessIp = await client.GetStringAsync("https://icanhazip.com");
                if (ip == proxylessIp)
                {
                    Log.Error("Webdriver Ip = {ip}, Proxyless Ip = {proxylessIp}", ip, proxylessIp);
                    throw new Exception("Webdriver is not getting to proxy.");
                }
            }
        }

        private async Task<WebDriver> SetupWebdriver()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            var chromeOptions = new ChromeOptions
            {
                Proxy = new Proxy()
                {
                    SocksProxy = "localhost:9050",
                    SocksVersion = 5
                }
            };
            chromeOptions.AddArgument("ignore-certificate-errors");
            // chromeOptions.AddArgument("headless");
            var driver = new ChromeDriver(chromeDriverService, chromeOptions);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(5);
            await TestWebdriverProxy(driver);
            return driver;
        }

        private void Login(WebDriver driver)
        {
            driver.Navigate().GoToUrl(LoginPage);
            driver.FindElement(By.Id("LoginPanel0_Username")).SendKeys(Username);
            driver.FindElement(By.Id("LoginPanel0_Password")).SendKeys(Password);
            driver.FindElement(By.Id("LoginButton")).Click();
            new WebDriverWait(driver, TimeSpan.FromMinutes(5)).Until(
                d => d.Url == "http://88.255.86.4:85/Lesson/Lesson");
            cookies[SessionCookie] = driver.Manage().Cookies.GetCookieNamed(SessionCookie).Value;
            cookies[AuthCookie] = driver.Manage().Cookies.GetCookieNamed(AuthCookie).Value;
            loggedIn = true;
        }

        private async Task<HttpResponseMessage> GetVideoById(HttpClient httpClient,
            ConcurrentBag<HttpResponseMessage> responses, int entityId)
        {
            int semaphoreCount;
            await semaphore.WaitAsync();
            try
            {
                var request = new HttpRequestMessage(new HttpMethod("POST"),
                    "http://88.255.86.4:85/services/Lesson/Video/Retrieve");

                var cookieString = new StringBuilder();
                foreach (var cookie in cookies)
                {
                    cookieString.Append($"{cookie.Key}={cookie.Value}; ");
                }

                request.Content = new StringContent($"{{\"EntityId\": \"{entityId}\" }}", Encoding.UTF8, "application/json");
                request.Headers.TryAddWithoutValidation("Cookie", cookieString.ToString());

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) responses.Add(response);
#if TIME
                Log.Debug("EntityId {EntityId} Request IsSuccessStatusCode {Success}", entityId, response.IsSuccessStatusCode);
#endif
                return response;
            }
            finally
            {
                semaphoreCount = semaphore.Release();
#if TIME
                Log.Debug("Request {Entity} releases semaphore, number of tasks that can enter semaphore: {Count}",
                    entityId, semaphoreCount);
#endif
            }
        }

        private static SemaphoreSlim semaphore;

        private async Task<List<TimeLecture>> ScrapeAllLessons()
        {
            semaphore = new SemaphoreSlim(50, 50);
#if TIME
            Log.Debug("Semaphore initialized. {Count} tasks can enter semaphore", semaphore.CurrentCount);
#endif
            var timeLectures = new List<TimeLecture>();
            {
                const int total = 1_000;
                const int start = 5_000;

                var tasks = new List<Task<HttpResponseMessage>>();
                var responses = new ConcurrentBag<HttpResponseMessage>();

                using var httpClient = new HttpClient(new HttpClientHandler()
                {
                    Proxy = new WebProxy("socks5://localhost:9050"),
                    UseCookies = false
                });

                foreach (var i in Enumerable.Range(start, total)) // iterate over a total of 20k urls
                {
                    tasks.Add(GetVideoById(httpClient, responses, i));
                }

                await Task.WhenAll(tasks);
                foreach (var response in responses)
                {
                    try
                    {
                        var str = await response.Content.ReadAsStringAsync();
                        JsonNode rootNode = JsonNode.Parse(str);
                        JsonObject entityNode = rootNode["Entity"].AsObject();

                        str = entityNode.ToJsonString();
                        TimeLecture timeLecture = JsonSerializer.Deserialize<TimeLecture>(str);
                        timeLectures.Add(timeLecture);
                    }
                    catch (Exception e)
                    {
#if TIME
                        Log.Error("JSON Deserialization of entity failed. {ErrorMessage}", e.Message);
#endif
                    }
                }

                return timeLectures;
            }
        }

        private string GetBreezeCookie(WebDriver driver)
        {
            try
            {
                if (!loggedIn) Login(driver);
                driver.Navigate().GoToUrl("http://88.255.86.4:85/Lesson/Video");
                var videoWatch = driver.FindElements(By.ClassName("video-watch"));
                foreach (var element in videoWatch)
                {
                    if (element.TagName == "a")
                    {
                        element.Click();
                        break;
                    }
                }

                var v = driver.FindElement(By.TagName("video"));
                var src = v.GetAttribute("src");
                BreezeCookie = src.Split("?")[-1].Split("=")[-1];
                return BreezeCookie;
            }
            catch (Exception e)
            {
                Log.Debug("Error while getting breeze cookie. {e}", e);
                return "";
            }
        }

        public async Task<string> GetBreezeCookie()
        {
            var cookieContainer = new CookieContainer();
            using var client = new HttpClient(new HttpClientHandler()
            {
                Proxy = new WebProxy("socks5://localhost:9050"),
                CookieContainer = cookieContainer
            });

            var uri = new Uri(
                "https://tustime.adobeconnect.com/api/xml?action=login&login=eslemaksu01@gmail.com&password=942658584");
            var response =
                await client.GetAsync(
                    "https://tustime.adobeconnect.com/api/xml?action=login&login=eslemaksu01@gmail.com&password=942658584");
            if (response.IsSuccessStatusCode)
            {
                var cookies = cookieContainer.GetCookies(uri).Cast<Cookie>();
                return cookies.First(x => x.Name == "BREEZESESSION").Value;
            }

            return "";
        }

        public async Task<IEnumerable<TimeLecture>> Scrape()
        {
            var driver = await SetupWebdriver();

            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(LoginPage);

            Login(driver);
            try
            {
                return await ScrapeAllLessons();
            }
            finally
            {
                driver.Close();
            }
        }
    }
}