using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.EntityFrameworkCore;
namespace Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // מסנן גלובלי שמסתיר קבצים שנמחקו רך
            modelBuilder.Entity<UserFile>().HasQueryFilter(f => !f.IsDeleted);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserFile> Files { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
    }
}
