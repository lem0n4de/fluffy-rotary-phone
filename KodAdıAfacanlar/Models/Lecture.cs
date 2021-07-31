using ReactiveUI;

namespace KodAdıAfacanlar.Models
{
    public class Lecture : ReactiveObject
    {
        private string _id;
        private string _title;
        private string _url;
        private string _teacher;

        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Url
        {
            get => _url;
            set => this.RaiseAndSetIfChanged(ref _url, value);
        }

        public string Teacher
        {
            get => _teacher;
            set => this.RaiseAndSetIfChanged(ref _teacher, value);
        }

        public Lecture(string title, string url, string teacher = "", string id = "")
        {
            Id = id;
            Title = title;
            Url = url;
            Teacher = teacher;
        }
    }
}