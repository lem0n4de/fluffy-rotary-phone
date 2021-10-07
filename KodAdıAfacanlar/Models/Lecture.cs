using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using KodAdıAfacanlar.Services;
using OpenQA.Selenium.DevTools.V91.Browser;
using ReactiveUI;

namespace KodAdıAfacanlar.Models
{
    public class Lecture : ReactiveObject
    {
        private int? lectureId;
        private string _title;
        private string _url;
        private string _teacher;
        private bool _downloaded;
        private bool _toDownload;
        private string _downloadPath;
        private string _javascriptCode;
        private int lessonId;
        private Lesson lesson;

        public Lesson Lesson
        {
            get => lesson;
            set => this.RaiseAndSetIfChanged(ref lesson, value);
        }
        public int LessonId
        {
            get => lessonId;
            set => this.RaiseAndSetIfChanged(ref lessonId, value);
        }

        public int? LectureId
        {
            get => lectureId;
            set => this.RaiseAndSetIfChanged(ref lectureId, value);
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

        public bool Downloaded
        {
            get => _downloaded;
            set => this.RaiseAndSetIfChanged(ref _downloaded, value);
        }

        public string DownloadPath
        {
            get => _downloadPath;
            set => this.RaiseAndSetIfChanged(ref _downloadPath, value);
        }

        public string GetNormalDownloadPath()
        {
            return DownloadPath.Replace("/", "-");
        }

        public bool ToDownload
        {
            get => _toDownload;
            set => this.RaiseAndSetIfChanged(ref _toDownload, value);
        }

        public string JavascriptCode
        {
            get => _javascriptCode;
            set => this.RaiseAndSetIfChanged(ref _javascriptCode, value);
        }

        public Lecture(string title, string url)
        {
            Title = title;
            Url = url;
            Teacher = "";
            Downloaded = false;
            DownloadPath = "";
            JavascriptCode = "";
        }

        private int _downloadProgress;

        [JsonIgnore]
        [NotMapped]
        public int DownloadProgress
        {
            get => _downloadProgress;
            set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
        }

        internal CancellationTokenSource TokenSource { get; set; }

        internal void CancelDownload()
        {
            TokenSource.Cancel();
        }

        internal void ProgressChangedEventHandler(object? sender, LectureDownloadProgressChangedEventArgs args)
        {
            if (args.Lecture != this) return;
            DownloadProgress = args.Progress;
        }

        internal void DownloadFinishedEventHandler(object sender, AsyncCompletedEventArgs eventArgs)
        {
            if (eventArgs.Cancelled)
            {
                try
                {
                    File.Delete(this.DownloadPath);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            if (eventArgs.Error != null)
            {
                Debug.WriteLine(eventArgs.Error.ToString());
            }
        }
    }
}