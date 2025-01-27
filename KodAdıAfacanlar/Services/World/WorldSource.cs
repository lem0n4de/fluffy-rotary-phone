﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using EFCore.BulkExtensions;
using KodAdıAfacanlar.Core;
using KodAdıAfacanlar.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KodAdıAfacanlar.Services.World
{
    public class WorldSource : Source
    {
        private ScrapingService scrapingService { get; }
        private HttpClient httpClient;

        public WorldSource()
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

        public override async Task<IEnumerable<Lesson>> GetLessonsOnlineAsync()
        {
            return await Task.Run(() => scrapingService.Scrape());
        }

        private async Task InitializeDbFromJson()
        {
            List<Lesson> lessonList = new();
            try
            {
                var str = File.OpenRead(Utils.GetContentFile("lessons.json"));
                lessonList = (await JsonSerializer.DeserializeAsync<IEnumerable<Lesson>>(str) ?? Array.Empty<Lesson>())
                    .ToList();

                using (var worldDatabase = new WorldDatabase())
                {
                    var l = new List<Lecture>();
                    await worldDatabase.BulkInsertAsync(lessonList, options => options.SetOutputIdentity = true);
                    using (var transaction = await worldDatabase.Database.BeginTransactionAsync())
                    {
                        foreach (var lesson in lessonList)
                        {
                            foreach (var lecture in lesson.LectureList)
                            {
                                lecture.LessonId = lesson.LessonId;
                                lecture.Lesson = lesson;
                            }

                            l.AddRange(lesson.LectureList);
                        }

                        await worldDatabase.BulkInsertAsync(l);
                        await transaction.CommitAsync();
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine(e);
            }
        }

        public override async Task<IEnumerable<Lesson>> GetLessonOfflineAsync()
        {
            using (var worldDatabase = new WorldDatabase())
            {
                if (!worldDatabase.Lessons.Any())
                {
                    await InitializeDbFromJson();
                }

                var l = worldDatabase.Lessons.Include(x => x.LectureList).ToList();
                foreach (var lesson in l)
                {
                    lesson.SyncListAndSource();
                }

                Log.Debug("GetLessonsIfflineAsync ended.");
                return l;
            }
        }

        public override async Task DownloadLectures(IEnumerable<Lecture> lectures)
        {
            using (var worldDatabase = new WorldDatabase())
            {
                foreach (var lecture in lectures.ToList())
                {
                    var lesson = await worldDatabase.Lessons.FirstAsync(x => x.LessonId == lecture.LessonId);
                    var lessonDownloadPath = lesson.GetDownloadPath("World");
                    if (!Directory.Exists(lessonDownloadPath))
                    {
                        Directory.CreateDirectory(lessonDownloadPath);
                    }

                    lecture.DownloadPath = Path.Combine(lessonDownloadPath, $"{lecture.Title}.mp4");
                    RaiseLectureDownloadProgressChangedEvent += lecture.ProgressChangedEventHandler;

                    var tokenSource = new CancellationTokenSource();
                    lecture.TokenSource = tokenSource;
                    lecture.EnableCancellation = true;

                    using var response =
                        await httpClient.GetAsync(lecture.Url, HttpCompletionOption.ResponseHeadersRead,
                            tokenSource.Token);
                    var length = response.Content.Headers.ContentLength;
                    if (length == null) length = 0L;

                    lecture.Downloaded = false;
                    try
                    {
                        await using (var source = await response.Content.ReadAsStreamAsync(tokenSource.Token))
                        {
                            await using (var destination = File.Open(lecture.GetNormalDownloadPath(), FileMode.Create))
                            {
                                // await source.CopyToAsync(destination, tokenSource.Token);
                                await CopyStream(lecture, source, destination, int.Parse(length.ToString()!),
                                    tokenSource.Token);
                            }
                        }
                        lecture.Downloaded = true;
                    }
                    catch (TaskCanceledException e)
                    {
                        // pass
                    }

                }

                await worldDatabase.SaveChangesAsync();
            }
        }

        public override void OnClose(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Log.Debug("WorldSource.OnClose");
        }
    }
}