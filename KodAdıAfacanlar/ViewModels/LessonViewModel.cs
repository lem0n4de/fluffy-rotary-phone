using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using KodAdıAfacanlar.Models;
using ReactiveUI;

namespace KodAdıAfacanlar.ViewModels
{
    public class LessonViewModel : ViewModelBase
    {
        private Lesson lesson;
        public Lesson Lesson
        {
            get => lesson;
            set => this.RaiseAndSetIfChanged(ref lesson, value);
        }

        private ReadOnlyObservableCollection<string> teacherList;
        public ReadOnlyObservableCollection<string> TeacherList
        {
            get => teacherList;
            set => this.RaiseAndSetIfChanged(ref teacherList, value);
        }

        private string selectedTeacher;
        public string SelectedTeacher
        {
            get => selectedTeacher;
            set => this.RaiseAndSetIfChanged(ref selectedTeacher, value);
        }

        private bool filterState;

        public bool FilterState
        {
            get => filterState;
            set => this.RaiseAndSetIfChanged(ref filterState, value);
        }

        private ObservableCollection<Lecture> filteredLectureList;
        public ObservableCollection<Lecture> FilteredLectureList
        {
            get => filteredLectureList;
            set => this.RaiseAndSetIfChanged(ref filteredLectureList, value);
        }
        
        public LessonViewModel(Lesson l)
        {
            Lesson = l;
            FilteredLectureList = new();
            FilteredLectureList.AddRange(Lesson.LectureSource.Items);
            FilterState = false;
            Lesson.LectureSource.Connect()
                .Transform(x => x.Teacher)
                .Filter(x => !string.IsNullOrEmpty(x))
                .DistinctValues(x => x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out teacherList)
                .Subscribe();

            this.WhenAnyValue(x => x.FilterState)
                .Subscribe(x =>
                {
                    if (x) return;
                    FilteredLectureList.Clear();
                    FilteredLectureList.AddRange(Lesson.LectureSource.Items);
                });

            this.WhenAnyValue(x => x.SelectedTeacher)
                .Subscribe(x =>
                {
                    if (string.IsNullOrEmpty(x)) return;
                    var f = Lesson.LectureSource.Items.Where(y => y.Teacher == x);
                    FilteredLectureList.Clear();
                    foreach (var l in f)
                    {
                        FilteredLectureList.Add(l);
                    }
                });
        }
    }
}