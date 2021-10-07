using KodAdıAfacanlar.Core;
using KodAdıAfacanlar.Models;
using Microsoft.EntityFrameworkCore;

namespace KodAdıAfacanlar.Services.Time
{
    public class TimeDatabase: DbContext
    {
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        
        public string DbPath { get; private set; }

        public TimeDatabase()
        {
            DbPath = Utils.GetContentFile("time.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite($"Data Source={DbPath}").EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Lesson>()
                .HasMany(lesson => lesson.LectureList)
                .WithOne(lecture => lecture.Lesson)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}