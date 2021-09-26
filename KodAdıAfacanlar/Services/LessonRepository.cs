using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using KodAdıAfacanlar.Models;

namespace KodAdıAfacanlar.Services
{
    public class LessonRepository
    {
        private ScrapingService scrapingService { get; }
        private string LessonsDbPath = @"lessons.json";
        private string ConfigPath = @"config.json";
        private HttpClient httpClient;

        public LessonRepository()
        {
            scrapingService = new ScrapingService();
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("authority", "www.tusworld.com.tr");
            httpClient.DefaultRequestHeaders.Add("scheme", "https");
            httpClient.DefaultRequestHeaders.Add("user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("referer", "https://www.tusworld.com.tr/VideoGrupDersleri");
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
                var str = File.ReadAllText(ConfigManager.GetLessonDbPath());
                var lessonList = JsonSerializer.Deserialize<IEnumerable<Lesson>>(str);
                foreach (var lesson in lessonList)
                {
                    lesson.SyncListAndSource();
                }

                return lessonList;
            }
            catch (FileNotFoundException e)
            {
                File.Create(ConfigManager.GetLessonDbPath()).Close();
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
                .SelectMany(lesson => lesson.LectureSource.Items.Where(lecture => lecture.ToDownload == true)))
            {
                lecture.ToDownload = false;
            }

            var str = JsonSerializer.Serialize(lessons);
            File.WriteAllText(ConfigManager.GetLessonDbPath(), str);
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
                foreach (var lecture in lesson.LectureSource.Items.Where(x => x.ToDownload))
                {

                    lecture.DownloadPath = Path.Combine(lesson.GetDownloadPath(), $"{lecture.Title}.mp4");
                    RaiseLectureDownloadProgressChangedEvent += lecture.ProgressChangedEventHandler;

                    var tokenSource = new CancellationTokenSource();
                    lecture.TokenSource = tokenSource;

                    using var response =
                        await httpClient.GetAsync(lecture.Url, HttpCompletionOption.ResponseHeadersRead);
                    var length = response.Content.Headers.ContentLength;
                    if (length == null) length = 0L;

                    await using (var source = await response.Content.ReadAsStreamAsync())
                    {
                        await using (var streamToWrite = File.Open(lecture.DownloadPath, FileMode.Create))
                        {
                            await CopyStream(lecture, source, streamToWrite, int.Parse(length.ToString()!),
                                tokenSource.Token);
                        }
                    }
                }
            }
        }

        private async Task CopyStream(Lecture lecture, Stream source, Stream destination, int sourceLength,
            CancellationToken token,
            int bufferSize = (16 * 1024))
        {
            var buffer = new byte[bufferSize];
            if (sourceLength <= 0) return;
            var totalBytesCopied = 0;
            var bytesRead = -1;
            while (bytesRead != 0)
            {
                bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytesRead == 0 || token.IsCancellationRequested) break;
                await destination.WriteAsync(buffer, 0, buffer.Length, token);
                totalBytesCopied += bytesRead;
                var progress =
                    (int) Math.Round(100.0 * totalBytesCopied / sourceLength); // Dont use int, it can overflow
                RaiseLectureDownloadProgressChangedEvent(null,
                    new LectureDownloadProgressChangedEventArgs(lecture, progress));
            }
        }

        public event EventHandler<LectureDownloadProgressChangedEventArgs> RaiseLectureDownloadProgressChangedEvent;

        public async Task DownloadLectures(IEnumerable<Lesson> lessons)
        {
            await DownloadLectures(lessons, null);
        }
    }
}