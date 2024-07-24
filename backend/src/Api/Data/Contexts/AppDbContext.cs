using Fei.Is.Api.Data.Configuration;
using Fei.Is.Api.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Contexts;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeStepConfiguration());
        modelBuilder.ApplyConfiguration(new JobStatusConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceTemplateConfiguration());
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<DeviceTemplate> DeviceTemplates { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobStatus> JobStatuses { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeStep> RecipeSteps { get; set; }
    public DbSet<Command> Commands { get; set; }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseModel>().Where(e => e.State == EntityState.Modified);
        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
