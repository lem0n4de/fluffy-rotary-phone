using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using KodAdıAfacanlar.Models;
using KodAdıAfacanlar.Services;

namespace KodAdıAfacanlar.Core
{
    public abstract class Source
    {
        public abstract Task<IEnumerable<Lesson>> GetLessonsOnlineAsync();
        public abstract Task<IEnumerable<Lesson>> GetLessonOfflineAsync();
        public abstract Task DownloadLectures(IEnumerable<Lecture> lectures);
        
        public event EventHandler<LectureDownloadProgressChangedEventArgs> RaiseLectureDownloadProgressChangedEvent;
        
        protected async Task CopyStream(Lecture lecture, Stream source, Stream destination, int sourceLength,
            CancellationToken token,
            int bufferSize = (16 * 1024))
        {
            var buffer = new byte[bufferSize];
            if (sourceLength <= 0) return;
            var totalBytesCopied = 0;
            var bytesRead = -1;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            while (bytesRead != 0)
            {
                bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytesRead == 0 || token.IsCancellationRequested) break;
                await destination.WriteAsync(buffer, 0, buffer.Length, token);
                totalBytesCopied += bytesRead;
                var progress =
                    (int) Math.Round(100.0 * totalBytesCopied / sourceLength); // Dont use int, it can overflow
                RaiseLectureDownloadProgressChangedEvent(null,
                    new LectureDownloadProgressChangedEventArgs(lecture, progress));
            }
        }
    }
}