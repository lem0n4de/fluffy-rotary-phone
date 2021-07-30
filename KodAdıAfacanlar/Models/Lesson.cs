using System.Collections.Generic;

namespace KodAdıAfacanlar.Models
{
    public class Lesson
    {
        public string Id;
        public string Title;
        public string JavascriptCode;
        public List<Lecture> LectureList;

        public Lesson(string title, string javascriptCode, string id = "")
        {
            Title = title;
            JavascriptCode = javascriptCode;
            Id = id;
            LectureList = new List<Lecture>();
        }
    }
}