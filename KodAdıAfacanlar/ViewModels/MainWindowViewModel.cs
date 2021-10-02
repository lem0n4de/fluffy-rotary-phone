using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using DynamicData;
using KodAdıAfacanlar.Core;
using KodAdıAfacanlar.Models;
using KodAdıAfacanlar.Services;
using KodAdıAfacanlar.Services.World;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace KodAdıAfacanlar.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Source source;

        public MainWindowViewModel()
        {
            source = new WorldSource();

            FetchLessonsCommand = ReactiveCommand.CreateFromTask(fetchLessons);
            DownloadLectures = ReactiveCommand.CreateFromTask(downloadLectures);
            Task.Run(loadLessonsAtStart);

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Exit += OnApplicationShutdown;
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
                LectureDownloadingList.AddRange(lesson.LectureSource.Items.Where(x => x.ToDownload));
            }

            await source.DownloadLectures(LectureDownloadingList);
        }

        public IReactiveCommand FetchLessonsCommand { get; }

        private async Task fetchLessons()
        {
            IsBusy = true;
            Lessons.Clear();
            Lessons2.Clear();
            var l = await source.GetLessonsOnlineAsync();
            if (l == null || !l.Any()) return;
            Lessons.AddRange(l);
            foreach (var lesson in Lessons)
            {
                Lessons2.Add(new LessonViewModel(lesson));
            }

            IsBusy = false;
        }

        private async Task loadLessonsAtStart()
        {
            IsBusy = true;
            var l = await source.GetLessonOfflineAsync();
            if (l == null || !l.Any())
            {
                IsBusy = false;
                return;
            }

            Lessons.AddRange(l);
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