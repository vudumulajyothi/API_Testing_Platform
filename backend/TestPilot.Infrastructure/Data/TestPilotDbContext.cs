using Microsoft.EntityFrameworkCore;
using TestPilot.Application.Interfaces;
using TestPilot.Domain.Entities;

namespace TestPilot.Infrastructure.Data;

public class TestPilotDbContext : DbContext, IApplicationDbContext
{
    public TestPilotDbContext(DbContextOptions<TestPilotDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<ApiRequest> ApiRequests => Set<ApiRequest>();
    public DbSet<RequestHeader> RequestHeaders => Set<RequestHeader>();
    public DbSet<QueryParameter> QueryParameters => Set<QueryParameter>();
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<TestRun> TestRuns => Set<TestRun>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<RequestHistory> RequestHistories => Set<RequestHistory>();
    public DbSet<AiSuggestion> AiSuggestions => Set<AiSuggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Name).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        // Collections
        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(c => c.CollectionId);
            entity.Property(c => c.Name).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500);

            entity.HasOne(c => c.User)
                  .WithMany(u => u.Collections)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ApiRequests
        modelBuilder.Entity<ApiRequest>(entity =>
        {
            entity.HasKey(r => r.RequestId);
            entity.Property(r => r.Name).HasMaxLength(150).IsRequired();
            entity.Property(r => r.Url).HasMaxLength(2048).IsRequired();
            entity.Property(r => r.Method).HasConversion<string>().HasMaxLength(10).IsRequired();

            entity.HasOne(r => r.Collection)
                  .WithMany(c => c.Requests)
                  .HasForeignKey(r => r.CollectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // RequestHeaders
        modelBuilder.Entity<RequestHeader>(entity =>
        {
            entity.HasKey(h => h.HeaderId);
            entity.Property(h => h.HeaderKey).HasMaxLength(150).IsRequired();
            entity.Property(h => h.HeaderValue).HasMaxLength(1000).IsRequired();

            entity.HasOne(h => h.Request)
                  .WithMany(r => r.Headers)
                  .HasForeignKey(h => h.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // QueryParameters
        modelBuilder.Entity<QueryParameter>(entity =>
        {
            entity.HasKey(p => p.ParameterId);
            entity.Property(p => p.ParamKey).HasMaxLength(150).IsRequired();
            entity.Property(p => p.ParamValue).HasMaxLength(1000).IsRequired();

            entity.HasOne(p => p.Request)
                  .WithMany(r => r.QueryParams)
                  .HasForeignKey(p => p.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TestCases
        modelBuilder.Entity<TestCase>(entity =>
        {
            entity.HasKey(t => t.TestCaseId);
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.AssertionType).HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(t => t.ExpectedValue).HasMaxLength(500).IsRequired();
            entity.Property(t => t.JsonPath).HasMaxLength(200);

            entity.HasOne(t => t.Request)
                  .WithMany(r => r.TestCases)
                  .HasForeignKey(t => t.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TestRuns
        modelBuilder.Entity<TestRun>(entity =>
        {
            entity.HasKey(tr => tr.TestRunId);

            entity.HasOne(tr => tr.Request)
                  .WithMany(r => r.TestRuns)
                  .HasForeignKey(tr => tr.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TestResults
        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(res => res.ResultId);
            entity.Property(res => res.ActualValue).HasMaxLength(500);
            entity.Property(res => res.ErrorMessage).HasMaxLength(1000);

            entity.HasOne(res => res.TestRun)
                  .WithMany(tr => tr.TestResults)
                  .HasForeignKey(res => res.TestRunId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(res => res.TestCase)
                  .WithMany(tc => tc.TestResults)
                  .HasForeignKey(res => res.TestCaseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // RequestHistory
        modelBuilder.Entity<RequestHistory>(entity =>
        {
            entity.HasKey(rh => rh.HistoryId);
            entity.Property(rh => rh.Method).HasConversion<string>().HasMaxLength(10).IsRequired();
            entity.Property(rh => rh.Url).HasMaxLength(2048).IsRequired();

            entity.HasOne(rh => rh.User)
                  .WithMany(u => u.RequestHistories)
                  .HasForeignKey(rh => rh.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rh => rh.Request)
                  .WithMany()
                  .HasForeignKey(rh => rh.RequestId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // AiSuggestions
        modelBuilder.Entity<AiSuggestion>(entity =>
        {
            entity.HasKey(s => s.SuggestionId);
            entity.Property(s => s.SuggestionText).IsRequired();

            entity.HasOne(s => s.Request)
                  .WithMany(r => r.AiSuggestions)
                  .HasForeignKey(s => s.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
