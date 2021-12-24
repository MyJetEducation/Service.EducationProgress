using Microsoft.EntityFrameworkCore;
using MyJetWallet.Sdk.Postgres;
using MyJetWallet.Sdk.Service;
using Service.EducationProgress.Domain.Models;

namespace Service.EducationProgress.Postgres
{
    public class DatabaseContext : MyDbContext
    {
        public const string Schema = "education";
        private const string EducationProgressTableName = "educationprogress";

        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<EducationProgressEntity> AssetsDictionarEntities { get; set; }

        public static DatabaseContext Create(DbContextOptionsBuilder<DatabaseContext> options)
        {
            MyTelemetry.StartActivity($"Database context {Schema}")?.AddTag("db-schema", Schema);

            return new DatabaseContext(options.Options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            SetUserInfoEntityEntry(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void SetUserInfoEntityEntry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EducationProgressEntity>().ToTable(EducationProgressTableName);
            modelBuilder.Entity<EducationProgressEntity>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EducationProgressEntity>().Property(e => e.Date).IsRequired();
            modelBuilder.Entity<EducationProgressEntity>().Property(e => e.Value);
            modelBuilder.Entity<EducationProgressEntity>().HasKey(e => e.Id);
        }
    }
}
