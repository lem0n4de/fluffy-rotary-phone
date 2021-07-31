using System.Collections.Generic;
using ReactiveUI;

namespace KodAdıAfacanlar.Models
{
    public class Lesson : ReactiveObject
    {
        private string id;
        private string title;
        private string javascriptCode;
        private List<Lecture> lectureList;

        public string Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        public string JavascriptCode
        {
            get => javascriptCode;
            set => this.RaiseAndSetIfChanged(ref javascriptCode, value);
        }

        public List<Lecture> LectureList
        {
            get => lectureList;
            set => this.RaiseAndSetIfChanged(ref lectureList, value);
        }

        public Lesson(string title, string javascriptCode, string id = "")
        {
            Title = title;
            JavascriptCode = javascriptCode;
            Id = id;
            LectureList = new List<Lecture>();
        }
    }
}