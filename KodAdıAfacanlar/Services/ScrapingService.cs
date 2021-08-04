using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KodAdıAfacanlar.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

namespace KodAdıAfacanlar.Services
{
    public class ScrapingService
    {
        private IWebDriver? Driver { get; set; }

        private void ScrapeLectures(Lesson lesson, string teacher = "")
        {
            var lectureElementList = new List<(string LectureName, string LectureId)>();
            var derslerListesi = Driver.FindElement(By.ClassName("DerslerListesi")).FindElements(By.TagName("a"));
            if (derslerListesi != null || !derslerListesi.Any())
            {
                foreach (var ders in derslerListesi)
                {
                    lectureElementList.Add((LectureName: ders.Text, LectureId: ders.GetAttribute("id")));
                }

                foreach (var lectureElement in lectureElementList)
                {
                    Driver.FindElement(By.Id(lectureElement.LectureId)).Click();
                    var videoSrc = Driver.FindElement(By.Id("Vid")).GetAttribute("src");
                    lesson.LectureList.Add(!String.IsNullOrEmpty(teacher)
                        ? new Lecture(lectureElement.LectureName, videoSrc) {Teacher = teacher}
                        : new Lecture(lectureElement.LectureName, videoSrc));
                }
            }
            else
            {
                Debug.WriteLine("DerslerListesi is null or empty.");
            }
        }

        public IEnumerable<Lesson> Scrape(bool onlySessionId = false)
        {
            Driver = new ChromeDriver();
            Driver.Manage().Window.Maximize();
            try
            {
                Driver.Navigate().GoToUrl("https://www.tusworld.com.tr/UyeGirisi");

                WebDriverWait wait = new WebDriverWait(Driver, new TimeSpan(0, 0, 5, 0));
                wait.Until(d => d.Url == "https://www.tusworld.com.tr/Anasayfa" ? true : false);
                ConfigManager.config.LastKnownSessionId =
                    Driver.Manage().Cookies.GetCookieNamed("ASP.NET_SessionId").Value;
                if (onlySessionId == true) return new List<Lesson>();

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
                    l.Add(new Lesson(title, id));

                    // Search for ids and find and click them via actions
                    // TRY to get HcAtf or HocaAlt elements and their ids
                    // Get each teacher and click them via actions
                    // Get DerslerListesi and FindElements By.TagName a and get their ids.
                    // Click on each one, wait, find element with id="Vid" and take get its src.
                    // Create a Lecture and add it to lesson's LectureList.
                }

                foreach (var lesson in l)
                {
                    Driver.FindElement(By.Id(lesson.HtmlId)).FindElement(By.TagName("span")).Click();

                    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

                    var hocaNameAndId = new List<(string HocaName, string HocaId)>();

                    try
                    {
                        var hcAtf = Driver.FindElement(By.ClassName("HcAtf"));
                        hocaNameAndId.Add((HocaName: hcAtf.Text, HocaId: hcAtf.GetAttribute("id")));
                        var hocaAlts = Driver.FindElements(By.ClassName("HocaAlt"));
                        foreach (var hocaAlt in hocaAlts)
                        {
                            hocaNameAndId.Add((HocaName: hocaAlt.Text, HocaId: hocaAlt.GetAttribute("id")));
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("No HcAtf or HocaAlt found.");
                    }

                    if (!hocaNameAndId.Any())
                    {
                        ScrapeLectures(lesson);
                    }
                    else
                    {
                        foreach (var hoca in hocaNameAndId)
                        {
                            Driver.FindElement(By.Id(hoca.HocaId)).Click();
                            ScrapeLectures(lesson, teacher: hoca.HocaName);
                        }
                    }
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
        }
    }
}