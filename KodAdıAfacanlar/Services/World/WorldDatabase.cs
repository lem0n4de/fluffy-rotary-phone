using System;
using System.Diagnostics;
using System.IO;
using KodAdıAfacanlar.Models;
using Microsoft.EntityFrameworkCore;

namespace KodAdıAfacanlar.Services.World
{
    public class WorldDatabase: DbContext
    {
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public string DbPath { get; private set; }

        public WorldDatabase()
        {
            DbPath = $"world.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite($"Data Source={DbPath}").EnableSensitiveDataLogging();
        }
    }
}