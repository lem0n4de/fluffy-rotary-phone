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
using KodAdıAfacanlar.Models;
using KodAdıAfacanlar.Services;
using ReactiveUI;

namespace KodAdıAfacanlar.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            FetchLessonsCommand = ReactiveCommand.CreateFromTask(_fetchLessons);
            DownloadLectures = ReactiveCommand.CreateFromTask(_downloadLectures);
            Task.Run(_loadLessonsAtStart);

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Exit += OnApplicationShutdown;
            }
        }

        public ObservableCollection<Lecture> LectureDownloadingList { get; set; } = new();

        private void OnApplicationShutdown(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            lessonRepository.SaveState(Lessons);
        }
        
        private LessonRepository lessonRepository { get; } = new();
        
        public IReactiveCommand DownloadLectures { get; }

        private async Task _downloadLectures()
        {
            ShowDownloads = true;
            // if (string.IsNullOrEmpty(ConfigManager.config.LastKnownSessionId))
            // {
            //     await lessonRepository.GetLessons(onlySessionId: true);
            // }
            foreach (var lesson in Lessons)
            {
                LectureDownloadingList.AddRange(lesson.LectureSource.Items.Where(x => x.ToDownload));
            }
            await lessonRepository.DownloadLectures(Lessons);
        }

        private void DownloadProgressTracker(object sender, DownloadProgressChangedEventArgs args)
        {
            Debug.WriteLine($"{args.UserState as string} | {args.ProgressPercentage} | {args.TotalBytesToReceive}");
        }
        public IReactiveCommand FetchLessonsCommand { get; }

        private async Task _fetchLessons()
        {
            IsBusy = true;
            Lessons.Clear();
            Lessons2.Clear();
            var l = await lessonRepository.GetLessons(forceScrape: true);
            if (l == null || !l.Any()) return;
            Lessons.AddRange(l);
            foreach (var lesson in Lessons)
            {
                Lessons2.Add(new LessonViewModel(lesson));
            }
            IsBusy = false;
        }
        private async Task _loadLessonsAtStart()
        {
            IsBusy = true;
            var l = await lessonRepository.GetLessons();
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