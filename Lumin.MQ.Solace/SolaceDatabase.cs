using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lumin.MQ.Solace
{
    public class SolaceDatabase : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=App_Data/SolaceStatistics.db");
        }

        public DbSet<ReceivedDto> ReceivedDtos { get; set; }
    }

    public class ReceivedDto
    {
        public Guid Id { get; set; }
        public string TransId { get; set; }
        public string Body { get; set; }
        public string SourceType { get; set; }
        public string SourceName { get; set; }
        public DateTime ReceivedTime { get; set; }
        public DateTime ProcessedTime { get; set; }
        public bool Processed { get; set; }
        public string Reply { get; set; }
    }
}
