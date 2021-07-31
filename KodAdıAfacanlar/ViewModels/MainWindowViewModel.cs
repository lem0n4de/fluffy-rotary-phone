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
        public IReactiveCommand FetchLessonsCommand { get; }

        private async Task _fetchLessons()
        {
            IsBusy = true;
            var l = await scrapingService.Scrape();
            if (!l.Any()) return;
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