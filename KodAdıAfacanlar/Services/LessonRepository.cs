using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using KodAdıAfacanlar.Models;

namespace KodAdıAfacanlar.Services
{
    public class LessonRepository
    {
        private ScrapingService scrapingService { get; }
        private string LessonsDbPath = @"lessons.json";
        private string ConfigPath = @"config.json";

        public LessonRepository()
        {
            scrapingService = new ScrapingService();
        }

        public async Task<IEnumerable<Lesson>> GetLessons(bool forceScrape = false, bool onlySessionId = false)
        {
            /* Check json file
             * if data in json return that
             * else scrape all
             */
            if (forceScrape)
            {
                var lessonsList = await GetLessonsViaScraping();
                SaveLessonsToDb(lessonsList);
                return lessonsList;
            }
            
            if (onlySessionId)
            {
                await GetSessionId();
            }

            var l = GetLessonsFromDb();

            if ((l == null || !l.Any()))
            {
                var lessonsList = await GetLessonsViaScraping();
                SaveLessonsToDb(lessonsList);
                return lessonsList;
            }

            return l;
        }

        public void SaveState(IEnumerable<Lesson> lessons)
        {
            SaveLessonsToDb(lessons);
            ConfigManager.SaveConfig();
        }

        private IEnumerable<Lesson>? GetLessonsFromDb()
        {
            try
            {
                var str = File.ReadAllText(LessonsDbPath);
                var lessonList = JsonSerializer.Deserialize<IEnumerable<Lesson>>(str);
                return lessonList;
            }
            catch (FileNotFoundException e)
            {
                File.Create(LessonsDbPath).Close();
                return null;
            }
        }

        private async Task<IEnumerable<Lesson>> GetLessonsViaScraping()
        {
            return await Task.Run(() => scrapingService.Scrape());
        }

        private async Task GetSessionId()
        {
            await Task.Run(() => scrapingService.Scrape(onlySessionId: true));
        }

        private void SaveLessonsToDb(IEnumerable<Lesson> lessons)
        {
            foreach (var lecture in lessons.ToList()
                .SelectMany(lesson => lesson.LectureList.Where(lecture => lecture.ToDownload == true)))
            {
                lecture.ToDownload = false;
            }

            var str = JsonSerializer.Serialize(lessons);
            File.WriteAllText(LessonsDbPath, str);
        }

        private void PrepareToDownload(IEnumerable<Lesson> lessons)
        {
            /*
             * Check if DownloadDirectory is not null
             * Check if directory "TUS" is existing in DownloadDirectory
             * If not existing, create
             * Create a directory for each lesson
             * Download each lecture to its lesson directory.
             */
            if (string.IsNullOrEmpty(ConfigManager.config.DownloadDirectory))
            {
                ConfigManager.config.DownloadDirectory =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "TUS");
            }

            if (!Directory.Exists(ConfigManager.config.DownloadDirectory))
            {
                Directory.CreateDirectory(ConfigManager.config.DownloadDirectory);
            }

            foreach (var lesson in lessons)
            {
                var lessonDir = Path.Combine(ConfigManager.config.DownloadDirectory, lesson.Title);
                if (!Directory.Exists(lessonDir))
                {
                    Directory.CreateDirectory(lessonDir);
                }
            }
        }

        public async Task DownloadLectures(IEnumerable<Lesson> lessons,
            DownloadProgressChangedEventHandler? eventHandler)
        {
            var l = lessons.ToList();
            PrepareToDownload(l);


            foreach (var lesson in l)
            {
                foreach (var lecture in lesson.LectureList.Where(x => x.ToDownload))
                {
                    using var client = new WebClient();
                    // client.Headers.Add("ASP.NET_SessionId", ConfigManager.config.LastKnownSessionId);
                    client.Headers.Add("authority", "www.tusworld.com.tr");
                    client.Headers.Add("scheme", "https");
                    client.Headers.Add("user-agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");
                    client.Headers.Add("accept", "*/*");
                    client.Headers.Add("referer", "https://www.tusworld.com.tr/VideoGrupDersleri");
                    client.Headers.Set("path", lecture.Url.Split("/").Last());
                    client.DownloadProgressChanged += lecture.ProgressChangedEventHandler;
                        
                    lecture.DownloadPath = Path.Combine(lesson.GetDownloadPath(), $"{lecture.Title}.mp4");
                    await client.DownloadFileTaskAsync(new Uri(lecture.Url), lecture.GetNormalDownloadPath());
                }
            }
        }

        public async Task DownloadLectures(IEnumerable<Lesson> lessons)
        {
            await DownloadLectures(lessons, null);
        }
    }
}