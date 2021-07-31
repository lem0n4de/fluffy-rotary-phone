using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using KodAdıAfacanlar.Models;
using ReactiveUI;

namespace KodAdıAfacanlar.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            FetchLessonsCommand = ReactiveCommand.Create(() =>
            {
                var l1 = new Lesson("title 1", "javascriptCode 1", "id 1");
                var lecture = new Lecture("title 1", "url 1", "teacher 1", "id 1");
                l1.LectureList.Add(lecture);
                Lessons.Add(l1);
            });
        }
        
        public IReactiveCommand FetchLessonsCommand { get; }
        public ObservableCollection<Lesson> Lessons { get; set; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
    }
}