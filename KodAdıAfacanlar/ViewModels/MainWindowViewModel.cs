using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
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
        }
        
        private ScrapingService scrapingService { get; } = new();
        private LessonRepository lessonRepository { get; } = new();
        public IReactiveCommand FetchLessonsCommand { get; }

        private async Task _fetchLessons()
        {
            // IsBusy = true;
            // Lessons.Clear();
            // for (var i = 0; i < 10; i++)
            // {
            //     var l = new Lesson($"title {i}", $"javascript code {i}", $"id {i}");
            //     for (var j = 0; j < 10; j++)
            //     {
            //         l.LectureList.Add(new Lecture($"lecture title {j}", $"lecture url {j}") { Teacher = $"lecture teacher {j}"});
            //     }
            //     Lessons.Add(l);
            // }
            IsBusy = true;
            Lessons.Clear();
            var l = await lessonRepository.GetLessons();
            if (l == null || !l.Any()) return;
            Lessons.AddRange(l);
            IsBusy = false;
        }
        public ObservableCollection<Lesson> Lessons { get; set; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
    }
}