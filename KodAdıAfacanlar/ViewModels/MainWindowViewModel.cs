using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using DynamicData;
using DynamicData.Binding;
using KodAdıAfacanlar.Core;
using KodAdıAfacanlar.Models;
using KodAdıAfacanlar.Services;
using KodAdıAfacanlar.Services.Time;
using KodAdıAfacanlar.Services.World;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using Serilog;

namespace KodAdıAfacanlar.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Source source;
        private LessonViewModel selectedLesson;

        public LessonViewModel SelectedLesson
        {
            get => selectedLesson;
            set => this.RaiseAndSetIfChanged(ref selectedLesson, value);
        }

        public MainWindowViewModel()
        {
#if TIME
            source = new TimeSource();
            Log.Debug("Time source initialized.");
#else
            source = new WorldSource();
            Log.Debug("World source initialized.");
#endif
            FetchLessonsCommand = ReactiveCommand.CreateFromTask(fetchLessons);
            DownloadLectures = ReactiveCommand.CreateFromTask(downloadLectures);
            Task.Run(loadLessonsAtStart);

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Exit += OnApplicationShutdown;
                desktopLifetime.Exit += source.OnClose;
            }
        }

        public ObservableCollection<Lecture> LectureDownloadingList { get; set; } = new();

        private void OnApplicationShutdown(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Debug.WriteLine($"{Application.Current.Name} is shutdown.");
        }

        public IReactiveCommand DownloadLectures { get; }

        private async Task downloadLectures()
        {
            ShowDownloads = true;
            foreach (var lesson in Lessons)
            {
                LectureDownloadingList.AddRange(lesson.LectureSource.Items.Where(x => x.ToDownload && !x.Downloaded));
            }

            foreach (var lecture in LectureDownloadingList.ToList())
            {
                lecture.WhenAnyValue(x => x.ToDownload).Subscribe(x =>
                {
                    if (x == false)
                    {
                        LectureDownloadingList.Remove(lecture);
                    }
                });
            }
            // Log.Debug("Downloading lectures: {@lectures}", LectureDownloadingList);

            await source.DownloadLectures(LectureDownloadingList);
        }

        public IReactiveCommand FetchLessonsCommand { get; }

        private async Task fetchLessons()
        {
            IsBusy = true;
            Lessons.Clear();
            Lessons2.Clear();
            var l = (await source.GetLessonsOnlineAsync()).ToList();
            if (!l.Any()) return;
            
            if (source is TimeSource)
            {
                var x = l.Where(lesson => (lesson.Title.Contains("Dönem 2022") || lesson.Title.Contains("CANLI YAYIN")) && lesson.LectureList.Count > 0).ToList();
                foreach (var lesson in x)
                {
                    lesson.Title = lesson.Title.Replace("Dönem 2022", "");
                    lesson.Title = lesson.Title.Replace("(", "");
                    lesson.Title = lesson.Title.Replace(")", "");
                }
                Lessons.AddRange(x);
            }
            else Lessons.AddRange(l);

            foreach (var lesson in Lessons)
            {
                Lessons2.Add(new LessonViewModel(lesson));
            }

            IsBusy = false;
        }

        private async Task loadLessonsAtStart()
        {
            IsBusy = true;
            var l = (await source.GetLessonOfflineAsync()).ToList();
            if (!l.Any())
            {
                IsBusy = false;
                return;
            }

            if (source is TimeSource)
            {
                var x = l.Where(lesson => (lesson.Title.Contains("Dönem 2022") || lesson.Title.Contains("CANLI YAYIN")) && lesson.LectureList.Count > 0).ToList();
                foreach (var lesson in x)
                {
                    lesson.Title = lesson.Title.Replace("Dönem 2022", "");
                    lesson.Title = lesson.Title.Replace("(", "");
                    lesson.Title = lesson.Title.Replace(")", "");
                }
                Lessons.AddRange(x);
            }
            else Lessons.AddRange(l);

            foreach (var lesson in Lessons)
            {
                Lessons2.Add(new LessonViewModel(lesson));
            }

            IsBusy = false;
        }

        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<LessonViewModel> Lessons2 { get; set; } = new();

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        private bool _showDownloads;

        public bool ShowDownloads
        {
            get => _showDownloads;
            set => this.RaiseAndSetIfChanged(ref _showDownloads, value);
        }
    }
}