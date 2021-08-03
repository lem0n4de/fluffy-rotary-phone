using System;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using ReactiveUI;

namespace KodAdıAfacanlar.Models
{
    public class Lecture : ReactiveObject
    {
        private string _id;
        private string _title;
        private string _url;
        private string _teacher;
        private bool _downloaded;
        private bool _toDownload;
        private string _downloadPath;
        private string _javascriptCode;

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
            Id = "";
            Teacher = "";
            Downloaded = false;
            DownloadPath = "";
            JavascriptCode = "";
        }

        private LectureDownloadProgress _downloadProgress;

        [JsonIgnore]
        public LectureDownloadProgress DownloadProgress
        {
            get => _downloadProgress;
            set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
        }

        internal void ProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs args)
        {
            DownloadProgress = new LectureDownloadProgress(this, args);
        }
    }
}