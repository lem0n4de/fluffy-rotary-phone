using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KodAdıAfacanlar.Services.Time
{
    public class TimeLecture
    {
        public int? VideoId { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? OnlineUrl { get; set; }
        public int? LessonId { get; set; }
        public string? LessonName { get; set; }
        public int? LessonGroupId { get; set; }
        public bool? LessonIsActive { get; set; }
        public DateTime? LessonInsertDate { get; set; }
        public int? LessonInsertUserId { get; set; }
        public DateTime? LessonUpdateDate { get; set; }
        public int? LessonUpdateUserId { get; set; }
        public int? InsertUserId { get; set; }
        public DateTime? InsertDate { get; set; }
        public int? UpdateUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? InsertUserDisplayName { get; set; }
        public string? UpdateUserDisplayName { get; set; }
    }

    public class TimeJson
    {
        [JsonPropertyName("videos")]
        public List<TimeLecture> Videos { get; set; }
    }
}