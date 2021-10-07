using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
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

        public abstract void OnClose(object? sender, ControlledApplicationLifetimeExitEventArgs e);

        public event EventHandler<LectureDownloadProgressChangedEventArgs> RaiseLectureDownloadProgressChangedEvent;

        protected async Task CopyStream(Lecture lecture, Stream source, Stream destination, int sourceLength,
            CancellationToken token,
            int bufferSize = (80 * 1024))
        {
            byte[] buffer = new byte[bufferSize];
            if (sourceLength <= 0) return;
            var totalBytesCopied = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
                totalBytesCopied += bytesRead;
                var progress = (int)Math.Round(100.0 * totalBytesCopied / sourceLength);
                RaiseLectureDownloadProgressChangedEvent(null,
                    new LectureDownloadProgressChangedEventArgs(lecture, progress));
            }
        }
    }
}