using System.Collections.Generic;
using ReactiveUI;

namespace KodAdıAfacanlar.Models
{
    public class Lesson : ReactiveObject
    {
        private string htmlId;
        private string title;
        private List<Lecture> lectureList;

        public string HtmlId
        {
            get => htmlId;
            set => this.RaiseAndSetIfChanged(ref htmlId, value);
        }

        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        public List<Lecture> LectureList
        {
            get => lectureList;
            set => this.RaiseAndSetIfChanged(ref lectureList, value);
        }

        public Lesson(string title, string htmlId)
        {
            Title = title;
            HtmlId = htmlId;
            LectureList = new List<Lecture>();
        }
    }
}