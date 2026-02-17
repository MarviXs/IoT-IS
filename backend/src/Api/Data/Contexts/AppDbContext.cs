using Fei.Is.Api.Data.Configuration;
using Fei.Is.Api.Data.Configuration.InformationSystem;
using Fei.Is.Api.Data.Configuration.LifeCycleSystem;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.System.Services;
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
    private readonly EdgeMetadataVersionService _edgeMetadataVersionService;
    private readonly EdgeMetadataDeltaTrackerService _edgeMetadataDeltaTrackerService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IMediator mediator,
        EdgeMetadataVersionService edgeMetadataVersionService,
        EdgeMetadataDeltaTrackerService edgeMetadataDeltaTrackerService
    )
        : base(options)
    {
        _mediator = mediator;
        _edgeMetadataVersionService = edgeMetadataVersionService;
        _edgeMetadataDeltaTrackerService = edgeMetadataDeltaTrackerService;
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
        modelBuilder.ApplyConfiguration(new DeviceFirmwareConfiguration());
        modelBuilder.ApplyConfiguration(new EdgeNodeConfiguration());
        modelBuilder.ApplyConfiguration(new CollectionItemConfiguration());
        modelBuilder.ApplyConfiguration(new SceneConfiguration());
        modelBuilder.ApplyConfiguration(new SceneSensorTriggerConfiguration());
        modelBuilder.ApplyConfiguration(new JobScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new JobScheduleWeekDayConfiguration());
        modelBuilder.ApplyConfiguration(new ExperimentConfiguration());

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
        modelBuilder.ApplyConfiguration(new OrderItemContainerConfiguration());
        modelBuilder.ApplyConfiguration(new VATCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSettingConfiguration());
        modelBuilder.ApplyConfiguration(new SystemNodeSettingConfiguration());
        modelBuilder.ApplyConfiguration(new UserFileConfiguration());

        //Life-cycle configurations
        modelBuilder.ApplyConfiguration(new PlantConfiguration());
        modelBuilder.ApplyConfiguration(new PlantAnalysisConfiguration());
        modelBuilder.ApplyConfiguration(new PlantBoardConfiguration());
    }

    //IoT tables
    public DbSet<DeviceTemplate> DeviceTemplates { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceFirmware> DeviceFirmwares { get; set; }
    public DbSet<EdgeNode> EdgeNodes { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobSchedule> JobSchedules { get; set; }
    public DbSet<JobScheduleWeekDay> JobScheduleWeekDays { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeStep> RecipeSteps { get; set; }
    public DbSet<Experiment> Experiments { get; set; }
    public DbSet<Command> Commands { get; set; }
    public DbSet<DeviceControl> DeviceControls { get; set; }
    public DbSet<JobCommand> JobCommands { get; set; }
    public DbSet<DeviceCollection> DeviceCollections { get; set; }
    public DbSet<CollectionItem> CollectionItems { get; set; }
    public DbSet<DeviceShare> DeviceShares { get; set; }
    public DbSet<Scene> Scenes { get; set; }
    public DbSet<SceneSensorTrigger> SceneSensorTriggers { get; set; }
    public DbSet<SceneNotification> SceneNotifications { get; set; }

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
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<SystemNodeSetting> SystemNodeSettings { get; set; }
    public DbSet<WorkDayDetail> WorkDayDetails { get; set; }
    public DbSet<WorkReport> WorkReports { get; set; }
    public DbSet<OrderItemContainer> OrderItemContainers { get; set; }
    public DbSet<UserFile> UserFiles { get; set; }

    //Life-cycle tables
    public DbSet<Plant> Plants { get; set; }
    public DbSet<PlantAnalysis> PlantAnalyses { get; set; }
    public DbSet<PlantBoard> PlantBoards { get; set; }
    public DbSet<GreenHouse> Greenhouses { get; set; }
    public DbSet<EditorBoard> EditorPots { get; set; }
    public DbSet<EditorPlant> EditorPlants { get; set; }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        var edgeMetadataChanges = CaptureEdgeMetadataChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
        var result = base.SaveChanges();
        if (result > 0 && edgeMetadataChanges.HasChanges)
        {
            TrySyncEdgeMetadataChangesAsync(edgeMetadataChanges, CancellationToken.None).GetAwaiter().GetResult();
        }

        return result;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        var edgeMetadataChanges = CaptureEdgeMetadataChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        if (result > 0 && edgeMetadataChanges.HasChanges)
        {
            TrySyncEdgeMetadataChangesAsync(edgeMetadataChanges, CancellationToken.None).GetAwaiter().GetResult();
        }

        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        await _mediator.DispatchDomainEventsAsync(this);
        var edgeMetadataChanges = await CaptureEdgeMetadataChangesAsync(cancellationToken);
        var result = await base.SaveChangesAsync(cancellationToken);
        if (result > 0 && edgeMetadataChanges.HasChanges)
        {
            await TrySyncEdgeMetadataChangesAsync(edgeMetadataChanges, cancellationToken);
        }

        return result;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        var edgeMetadataChanges = await CaptureEdgeMetadataChangesAsync(cancellationToken);
        return await SaveChangesAndSyncMetadataAsync(acceptAllChangesOnSuccess, edgeMetadataChanges, cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseModel>().Where(e => e.State == EntityState.Modified);
        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task<int> SaveChangesAndSyncMetadataAsync(
        bool acceptAllChangesOnSuccess,
        EdgeMetadataChangeSet edgeMetadataChanges,
        CancellationToken cancellationToken
    )
    {
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        if (result > 0 && edgeMetadataChanges.HasChanges)
        {
            await TrySyncEdgeMetadataChangesAsync(edgeMetadataChanges, cancellationToken);
        }

        return result;
    }

    private async Task<EdgeMetadataChangeSet> CaptureEdgeMetadataChangesAsync(CancellationToken cancellationToken)
    {
        var changes = new EdgeMetadataChangeSet();
        var recipeIdsToResolve = new HashSet<Guid>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            switch (entry.Entity)
            {
                case Device device:
                    RegisterDeviceChange(changes, device.Id, entry.State);
                    break;
                case DeviceTemplate template:
                    RegisterTemplateChange(changes, template.Id, entry.State);
                    break;
                case Sensor sensor:
                    RegisterTemplateChange(changes, sensor.DeviceTemplateId, EntityState.Modified);
                    break;
                case Command command:
                    RegisterTemplateChange(changes, command.DeviceTemplateId, EntityState.Modified);
                    break;
                case Recipe recipe:
                    RegisterTemplateChange(changes, recipe.DeviceTemplateId, EntityState.Modified);
                    break;
                case DeviceControl control:
                    RegisterTemplateChange(changes, control.DeviceTemplateId, EntityState.Modified);
                    break;
                case DeviceFirmware firmware:
                    RegisterTemplateChange(changes, firmware.DeviceTemplateId, EntityState.Modified);
                    break;
                case RecipeStep recipeStep:
                    if (recipeStep.Recipe?.DeviceTemplateId is { } trackedTemplateId && trackedTemplateId != Guid.Empty)
                    {
                        RegisterTemplateChange(changes, trackedTemplateId, EntityState.Modified);
                    }
                    else if (recipeStep.RecipeId != Guid.Empty)
                    {
                        recipeIdsToResolve.Add(recipeStep.RecipeId);
                    }

                    break;
            }
        }

        if (recipeIdsToResolve.Count == 0)
        {
            return changes;
        }

        var trackedRecipes = ChangeTracker
            .Entries<Recipe>()
            .Where(entry => entry.State != EntityState.Detached && recipeIdsToResolve.Contains(entry.Entity.Id))
            .Select(entry => entry.Entity)
            .ToList();

        foreach (var trackedRecipe in trackedRecipes)
        {
            if (trackedRecipe.DeviceTemplateId != Guid.Empty)
            {
                RegisterTemplateChange(changes, trackedRecipe.DeviceTemplateId, EntityState.Modified);
            }
        }

        recipeIdsToResolve.ExceptWith(trackedRecipes.Select(recipe => recipe.Id));
        if (recipeIdsToResolve.Count == 0)
        {
            return changes;
        }

        var resolvedTemplateIds = await Recipes
            .AsNoTracking()
            .Where(recipe => recipeIdsToResolve.Contains(recipe.Id))
            .Select(recipe => recipe.DeviceTemplateId)
            .ToListAsync(cancellationToken);

        foreach (var templateId in resolvedTemplateIds)
        {
            RegisterTemplateChange(changes, templateId, EntityState.Modified);
        }

        return changes;
    }

    private async Task TrySyncEdgeMetadataChangesAsync(EdgeMetadataChangeSet edgeMetadataChanges, CancellationToken cancellationToken)
    {
        try
        {
            if (!edgeMetadataChanges.HasChanges || !await IsEdgeNodeModeAsync(cancellationToken))
            {
                return;
            }

            await _edgeMetadataDeltaTrackerService.RecordChangesAsync(edgeMetadataChanges, cancellationToken);
            await _edgeMetadataVersionService.IncrementVersionAsync(cancellationToken);
        }
        catch
        {
            // Metadata version sync failures should not break primary database writes.
        }
    }

    private static void RegisterTemplateChange(EdgeMetadataChangeSet changes, Guid templateId, EntityState state)
    {
        if (templateId == Guid.Empty)
        {
            return;
        }

        if (state == EntityState.Deleted)
        {
            changes.DeletedTemplateIds.Add(templateId);
            changes.ChangedTemplateIds.Remove(templateId);
            return;
        }

        if (changes.DeletedTemplateIds.Contains(templateId))
        {
            return;
        }

        changes.ChangedTemplateIds.Add(templateId);
    }

    private static void RegisterDeviceChange(EdgeMetadataChangeSet changes, Guid deviceId, EntityState state)
    {
        if (deviceId == Guid.Empty)
        {
            return;
        }

        if (state == EntityState.Deleted)
        {
            changes.DeletedDeviceIds.Add(deviceId);
            changes.ChangedDeviceIds.Remove(deviceId);
            return;
        }

        changes.ChangedDeviceIds.Add(deviceId);
        changes.DeletedDeviceIds.Remove(deviceId);
    }

    private async Task<bool> IsEdgeNodeModeAsync(CancellationToken cancellationToken)
    {
        var trackedNodeSetting = ChangeTracker
            .Entries<SystemNodeSetting>()
            .Where(entry => entry.State != EntityState.Deleted)
            .Select(entry => (SystemNodeType?)entry.Entity.NodeType)
            .FirstOrDefault();
        if (trackedNodeSetting.HasValue)
        {
            return trackedNodeSetting.Value == SystemNodeType.Edge;
        }

        var persistedNodeType = await SystemNodeSettings
            .AsNoTracking()
            .OrderBy(setting => setting.CreatedAt)
            .Select(setting => (SystemNodeType?)setting.NodeType)
            .FirstOrDefaultAsync(cancellationToken);

        return (persistedNodeType ?? SystemNodeType.Hub) == SystemNodeType.Edge;
    }
}
