using System;
using KodAdıAfacanlar.Models;

namespace KodAdıAfacanlar.Services
{
    public class LectureDownloadProgressChangedEventArgs : EventArgs
    {
        public LectureDownloadProgressChangedEventArgs(Lecture lecture, int progress)
        {
            Lecture = lecture;
            Progress = progress;
        }
        public int Progress { get; set; }
        public Lecture Lecture;

    }
}