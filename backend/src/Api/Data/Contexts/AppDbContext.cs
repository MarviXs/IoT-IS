using Fei.Is.Api.Data.Configuration;
using Fei.Is.Api.Data.Configuration.InformationSystem;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Contexts;

public class AppDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        ApplicationUserRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>
    >
{
    private readonly IMediator _mediator;

    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationRoleConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeStepConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
        modelBuilder.ApplyConfiguration(new CollectionItemConfiguration());

        //IS configurations
        modelBuilder.ApplyConfiguration(new AdditionalOrderConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new DeliveryItemConfiguration());
        modelBuilder.ApplyConfiguration(new DeliveryNoteConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductionPlanConfiguration());
        modelBuilder.ApplyConfiguration(new SummaryConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
        modelBuilder.ApplyConfiguration(new WorkDayDetailConfiguration());
        modelBuilder.ApplyConfiguration(new WorkReportConfiguration());
        modelBuilder.ApplyConfiguration(new VATCategoryConfiguration());

        modelBuilder.ApplyDataSeeds();
    }

    //IoT tables
    public DbSet<DeviceTemplate> DeviceTemplates { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeStep> RecipeSteps { get; set; }
    public DbSet<Command> Commands { get; set; }
    public DbSet<JobCommand> JobCommands { get; set; }
    public DbSet<DeviceCollection> DeviceCollections { get; set; }
    public DbSet<CollectionItem> CollectionItems { get; set; }
    public DbSet<DeviceShare> DeviceShares { get; set; }

    //IS tables
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AdditionalOrder> AdditionalOrders { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<DeliveryItem> DeliveryItems { get; set; }
    public DbSet<DeliveryNote> DeliveryNotes { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<VATCategory> VATCategories { get; set; }
    public DbSet<ProductionPlan> ProductionPlans { get; set; }
    public DbSet<Summary> Summaries { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<WorkDayDetail> WorkDayDetails { get; set; }
    public DbSet<WorkReport> WorkReports { get; set; }

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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        await _mediator.DispatchDomainEventsAsync(this);
        return await base.SaveChangesAsync(cancellationToken);
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
