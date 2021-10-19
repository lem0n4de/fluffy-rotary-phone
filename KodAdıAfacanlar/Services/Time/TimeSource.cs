using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using KodAdıAfacanlar.Core;
using KodAdıAfacanlar.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KodAdıAfacanlar.Services.Time
{
    public class TimeSource : Source
    {
        private TimeTorService torService;
        private TimeScraper timeScraper;
        private HttpClient httpClient;

        public TimeSource()
        {
            torService = new TimeTorService();
            timeScraper = new TimeScraper();
            httpClient = new HttpClient(new HttpClientHandler()
            {
                Proxy = new WebProxy("socks5://localhost:9050")
            });
        }

        public override async Task<IEnumerable<Lesson>> GetLessonsOnlineAsync()
        {
            return await timeScraper.Scrape();
        }

        private async Task InitializeDbFromJson(TimeDatabase timeDatabase)
        {
            try
            {
                var str = await File.ReadAllTextAsync(Utils.GetContentFile("video-links.json"));
                var timeJson = JsonSerializer.Deserialize<TimeJson>(str);
                var videos = timeJson!.Videos;
                Log.Debug("Deserialized video-links.json");

                // Build a lessons list
                var lessonList = new List<Lesson>();
                foreach (var timeLecture in videos)
                {
                    if (!lessonList.Exists(x => x.LessonId == timeLecture.LessonId) && timeLecture.LessonName != null &&
                        timeLecture.LessonId != null)
                    {
                        lessonList.Add(new Lesson(timeLecture.LessonName!, "")
                        {
                            LessonId = timeLecture.LessonId ?? -1,
                            LectureList = new List<Lecture>()
                        });
                    }
                }

                // Convert all TimeLectures to Standart Lectures
                var lectureList = new List<Lecture>();
                foreach (var timeLecture in videos)
                {
                    if (timeLecture.LessonId == null || timeLecture.Url == null) continue;
                    lectureList.Add(new Lecture(timeLecture.Name!, timeLecture.Url!)
                    {
                        LessonId = (int) timeLecture.LessonId,
                        Lesson = lessonList.Find(lesson => lesson.LessonId == timeLecture.LessonId)!
                    });
                }

                // Add all lectures to the lessons they belong
                foreach (var lesson in lessonList)
                {
                    lesson.LectureList.AddRange(lectureList.FindAll(x => x.LessonId == lesson.LessonId));
                }

                await timeDatabase.AddRangeAsync(lessonList);
                await timeDatabase.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("Error in getting lessons from db or json. {e}", e);
                throw new Exception("Error in getting lessons from db or json.");
            }
        }

        public override async Task<IEnumerable<Lesson>> GetLessonOfflineAsync()
        {
            using (var timeDatabase = new TimeDatabase())
            {
                if (!timeDatabase.Lessons.Any())
                {
                    Log.Debug("No items in database, initializing from json.");
                    await InitializeDbFromJson(timeDatabase);
                }

                var l = timeDatabase.Lessons.Include(x => x.LectureList).ToList();
                foreach (var lesson in l)
                {
                    lesson.SyncListAndSource();
                }

                Log.Debug("Returning items from database.");

                return l;
            }
        }

        public override async Task DownloadLectures(IEnumerable<Lecture> lectures)
        {
            if (!OperatingSystem.IsMacOS())
            {
                await torService.SetupToolsAsync();
            }

            await torService.StartTorAsync();

            var breezeCookie = await timeScraper.GetBreezeCookie();

            using (var timeDatabase = new TimeDatabase())
            {
                foreach (var lecture in lectures)
                {
                    var lesson = await timeDatabase.Lessons.FirstAsync(x => x.LessonId == lecture.LessonId);
                    var lessonDownloadPath = lesson.GetDownloadPath("Time");
                    if (!Directory.Exists(lessonDownloadPath))
                    {
                        Directory.CreateDirectory(lessonDownloadPath);
                    }

                    lecture.DownloadPath = Path.Combine(lessonDownloadPath, $"{lecture.Title}.mp4");
                    RaiseLectureDownloadProgressChangedEvent += lecture.ProgressChangedEventHandler;

                    var tokenSource = new CancellationTokenSource();
                    lecture.TokenSource = tokenSource;

                    using var response = await httpClient.GetAsync($"{lecture.Url}?session={breezeCookie}",
                        HttpCompletionOption.ResponseHeadersRead, tokenSource.Token);
                    var length = response.Content.Headers.ContentLength;
                    if (length == null)
                    {
                        Log.Debug("Lesson to be downloaded has Content-Length 0 {@lecture}", lecture);
                        length = 0L;
                    }

                    lecture.Downloaded = false;

                    await using (var source = await response.Content.ReadAsStreamAsync(tokenSource.Token))
                    {
                        await using (var destination = File.Open(lecture.DownloadPath, FileMode.Create))
                        {
                            await CopyStream(lecture, source, destination, int.Parse(length.ToString()!),
                                tokenSource.Token);
                        }
                    }

                    lecture.Downloaded = true;
                }

                await timeDatabase.SaveChangesAsync();
            }
        }

        public override void OnClose(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            torService.Close();
            Log.Debug("TimeSource.OnClose");
        }
    }
}