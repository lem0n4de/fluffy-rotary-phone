using System;
using System.Net;

namespace KodAdıAfacanlar.Models
{
    public class LectureDownloadProgress
    {
        public Lecture Lecture { get; }
        public DownloadProgressChangedEventArgs EventArgs { get; }

        public LectureDownloadProgress(Lecture lecture, DownloadProgressChangedEventArgs eventArgs)
        {
            Lecture = lecture;
            EventArgs = eventArgs;
        }
    }
}