using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }


        public DbSet<User> Users { get; set; }
        public DbSet<UserFile> Files { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<Backup> Backups { get; set; }
        public DbSet<SpeechToText> SpeechToTexts { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }
}
