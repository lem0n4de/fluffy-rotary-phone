using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using KodAdıAfacanlar.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace KodAdıAfacanlar.Services
{
    public class ScrapingService
    {
        private IWebDriver Driver { get; set; }

        public async Task<IEnumerable<Lesson>> Scrape()
        {
            return await Task.Run(() =>
            {
                Driver = new ChromeDriver();
                Driver.Manage().Window.Maximize();
                try
                {
                    Driver.Navigate().GoToUrl("https://www.tusworld.com.tr/UyeGirisi");

                    WebDriverWait wait = new WebDriverWait(Driver, new TimeSpan(0, 0, 5, 0));
                    wait.Until(d => d.Url == "https://www.tusworld.com.tr/Anasayfa" ? true : false);

                    wait.Until(d => d.FindElement(By.Id("tclose"))).Click();
                    Driver.FindElement(By.ClassName("VdRnk")).Click();
                    wait.Until(d => d.FindElement(By.ClassName("Tusblue"))).Click();

                    // Evde offline butonuna tıklamak için bütün butonlardan texte göre filtreleyip doğru olana tıklıyoruz.
                    // Neden id kullanmıyorum? Her sayfada butonların idleri saçma sapan, ilerde değişirse diye böyle yaptım.
                    var d1 = wait.Until(d => d.FindElements(By.ClassName("VdDrKaSub")));
                    foreach (var element in d1)
                    {
                        if (element.Text.Contains("Evde") && element.Text.Contains("Offline"))
                            element.Click();
                        break;
                    }

                    // Kursiyerlere özel offline dersler butonunu tıklayıp bulmak için 'a' taglerini listeleyip texte göre filtrelemek lazım
                    // Şimdilik id'ye göre
                    wait.Until(d => d.Url == "https://www.tusworld.com.tr/VideoKategori");
                    Driver.FindElement(By.Id("ContentPlaceHolder1_rptVideoGrupKategori_lnkHref_3")).Click();
                    // var aList = Driver.FindElements(By.TagName("a"));
                    // foreach (var element in aList)
                    // {
                    //     Debug.WriteLine($"{element.Text}");
                    //     if (element.Text.Contains("Kursiyerlere Özel Offline Dersler"))
                    //         element.Click();
                    //     break;
                    // }

                    // TODO Save SessionId cookie

                    // Get Lesson List and build a database
                    wait.Until(d => d.Url == "https://www.tusworld.com.tr/VideoGrupDersleri");
                    var x = Driver.FindElement(By.ClassName("DersKategorileri"));
                    // Get all child elements in list and iterate over them to find lesson name and link and id
                    var y = x.FindElements(By.TagName("a"));

                    var l = new List<Lesson>();
                    foreach (var lesson in y)
                    {
                        var id = lesson.GetAttribute("id");
                        var title = lesson.Text;
                        var javascriptCode = lesson.GetAttribute("href");
                        l.Add(new Lesson(title, javascriptCode, id));
                    }

                    return l;

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return new List<Lesson>();
                }
                finally
                {
                    Driver.Quit();
                }
            });
        }
    }
}