using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.Json.Serialization;
using DynamicData;
using ReactiveUI;

namespace KodAdıAfacanlar.Models
{
    public class Lesson : ReactiveObject
    {
        private string htmlId;
        private string title;
        private List<Lecture> lectureList;
        private SourceList<Lecture> lectureSource;
        private int lessonId;

        public int LessonId
        {
            get => lessonId;
            set => this.RaiseAndSetIfChanged(ref lessonId, value);
        }

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

        [JsonIgnore]
        [NotMapped]
        public SourceList<Lecture> LectureSource
        {
            get => lectureSource;
            set => this.RaiseAndSetIfChanged(ref lectureSource, value);
        }

        public List<Lecture> LectureList
        {
            get => lectureList;
            set => this.RaiseAndSetIfChanged(ref lectureList, value);
        }

        internal void SyncListAndSource()
        {
            LectureSource.AddRange(LectureList);
        }

        public Lesson(string title, string htmlId)
        {
            Title = title;
            HtmlId = htmlId;
            LectureSource = new SourceList<Lecture>();
        }

        public string GetDownloadPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "TUS", Title);
        }
    }
}