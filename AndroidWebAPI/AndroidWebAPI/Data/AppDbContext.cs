using Microsoft.EntityFrameworkCore;
using AndroidWebAPI.Models;

namespace AndroidWebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Child> Children { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<VaccinationRecord> VaccinationRecords { get; set; }
        public DbSet<VaccinationTimeline> VaccinationTimelines { get; set; }

        public DbSet<VaccinationScheduleRule> VaccinationScheduleRules { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<VaccineDose> VaccineDoses { get; set; }
        public DbSet<VaccineInventory> VaccineInventory { get; set; }
        
        public DbSet<ChildParentRelationship> ChildParentRelationships { get; set; }
     

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChildParentRelationship>(entity =>
            {
                entity.ToTable("ChildParentRelationship");

                entity.HasKey(e => e.RelationshipID);

                entity.HasOne(e => e.Child)
                      .WithMany(c => c.ParentRelationships)
                      .HasForeignKey(e => e.ChildID);

                entity.HasOne(e => e.Parent)
                      .WithMany(p => p.ChildRelationships)
                      .HasForeignKey(e => e.ParentID);
            });

            modelBuilder.Entity<VaccinationRecord>()
                .HasKey(v => v.VaccinationRecordID);
            modelBuilder.Entity<VaccinationTimeline>()
                .HasKey(v => v.TimelineID);

            modelBuilder.Entity<VaccinationScheduleRule>()
                .HasKey(v => v.RuleID);


            modelBuilder.Entity<Vaccine>()
                .HasKey(v => v.VaccineID);

            modelBuilder.Entity<VaccineDose>()
                .HasKey(v => v.DoseID);

            base.OnModelCreating(modelBuilder);
        }
    }
}