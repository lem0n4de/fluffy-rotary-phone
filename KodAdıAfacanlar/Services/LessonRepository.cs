using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KodAdıAfacanlar.Models;

namespace KodAdıAfacanlar.Services
{
    public class LessonRepository
    {
        private ScrapingService scrapingService { get; }
        private string LessonsDbPath = @"lessons.json";

        public LessonRepository()
        {
            scrapingService = new ScrapingService();
        }

        public async Task<IEnumerable<Lesson>> GetLessons()
        {
            /* Check json file
             * if data in json return that
             * else scrape all
             */
            var l = GetLessonsFromDb();

            if (l == null || !l.Any())
            {
                var lessonsList = await GetLessonsViaScraping();
                SaveLessonsToDb(lessonsList);
                return lessonsList;
            }

            return l;
        }

        private IEnumerable<Lesson>? GetLessonsFromDb()
        {
            try
            {
                var str = File.ReadAllText(LessonsDbPath);
                var lessonList = JsonSerializer.Deserialize<IEnumerable<Lesson>>(str);
                return lessonList;
            }
            catch (FileNotFoundException e)
            {
                File.Create(LessonsDbPath).Close();
                return null;
            }
        }

        private async Task<IEnumerable<Lesson>> GetLessonsViaScraping()
        {
            return await Task.Run(() => scrapingService.Scrape());
        }

        private void SaveLessonsToDb(IEnumerable<Lesson> lessons)
        {
            var str = JsonSerializer.Serialize(lessons);
            File.WriteAllText(LessonsDbPath, str);
        }
    }
}