namespace KodAdıAfacanlar.Models
{
    public class Lecture
    {
        public string Id;
        public string Title;
        public string Url;
        public string Teacher;

        public Lecture(string title, string url, string teacher = "", string id = "")
        {
            Id = id;
            Title = title;
            Url = url;
            Teacher = teacher;
        }
    }
}