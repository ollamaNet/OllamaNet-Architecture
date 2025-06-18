# Code Examples: Ollama DB Layer

## Repository Pattern Implementation

### Generic Repository Interface

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(string id);
    Task<bool> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> SoftDeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}
```

### Concrete Repository Implementation

```csharp
public class TagRepository : ITagRepository
{
    private readonly MyDbContext _context;

    public TagRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _context.Tags
            .Where(t => !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<Tag> GetByIdAsync(string id)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<bool> AddAsync(Tag entity)
    {
        await _context.Tags.AddAsync(entity);
        return true;
    }

    public async Task<bool> UpdateAsync(Tag entity)
    {
        _context.Tags.Update(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null)
            return false;

        _context.Tags.Remove(tag);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(string id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null)
            return false;

        tag.IsDeleted = true;
        _context.Tags.Update(tag);
        return true;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Tags
            .AnyAsync(t => t.Id == id && !t.IsDeleted);
    }
}
```

## Unit of Work Pattern

### Unit of Work Interface

```csharp
public interface IUnitOfWork : IDisposable
{
    IAIModelRepository AIModels { get; }
    IAIResponseRepository AIResponses { get; }
    IAttachmentRepository Attachments { get; }
    IConversationPromptResponseRepository ConversationPromptResponses { get; }
    IConversationRepository Conversations { get; }
    IFeedbackRepository Feedbacks { get; }
    IModelTagRepository ModelTags { get; }
    IPromptRepository Prompts { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    ISystemMessageRepository SystemMessages { get; }
    ITagRepository Tags { get; }
    IUserRepository Users { get; }
    
    Task<int> SaveChangesAsync();
}
```

### Unit of Work Implementation

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly MyDbContext _context;
    private bool _disposed = false;

    public IAIModelRepository AIModels { get; }
    public IAIResponseRepository AIResponses { get; }
    public IAttachmentRepository Attachments { get; }
    public IConversationPromptResponseRepository ConversationPromptResponses { get; }
    public IConversationRepository Conversations { get; }
    public IFeedbackRepository Feedbacks { get; }
    public IModelTagRepository ModelTags { get; }
    public IPromptRepository Prompts { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public ISystemMessageRepository SystemMessages { get; }
    public ITagRepository Tags { get; }
    public IUserRepository Users { get; }

    public UnitOfWork(
        MyDbContext context,
        IAIModelRepository aiModelRepository,
        IAIResponseRepository aiResponseRepository,
        IAttachmentRepository attachmentRepository,
        IConversationPromptResponseRepository conversationPromptResponseRepository,
        IConversationRepository conversationRepository,
        IFeedbackRepository feedbackRepository,
        IModelTagRepository modelTagRepository,
        IPromptRepository promptRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ISystemMessageRepository systemMessageRepository,
        ITagRepository tagRepository,
        IUserRepository userRepository)
    {
        _context = context;
        AIModels = aiModelRepository;
        AIResponses = aiResponseRepository;
        Attachments = attachmentRepository;
        ConversationPromptResponses = conversationPromptResponseRepository;
        Conversations = conversationRepository;
        Feedbacks = feedbackRepository;
        ModelTags = modelTagRepository;
        Prompts = promptRepository;
        RefreshTokens = refreshTokenRepository;
        SystemMessages = systemMessageRepository;
        Tags = tagRepository;
        Users = userRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

## Entity Definition

### Entity with Relationships

```csharp
public class Conversation
{
    public string Id { get; set; }
    public string Title { get; set; }
    public int TokensUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsArchived { get; set; }
    public string Status { get; set; }
    public string SystemMessage { get; set; }
    
    // Foreign key
    public string UserId { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; }
    public virtual ICollection<ConversationPromptResponse> ConversationPromptResponses { get; set; }
    public virtual ICollection<Attachment> Attachments { get; set; }
}
```

### Extended Identity User

```csharp
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsDeleted { get; set; }
    public int TokenVersion { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<AIModel> AIModels { get; set; }
    public virtual ICollection<Conversation> Conversations { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
}
```

## Database Context

```csharp
public class MyDbContext : IdentityDbContext<ApplicationUser>
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<AIModel> AIModels { get; set; }
    public DbSet<AIResponse> AIResponses { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationPromptResponse> ConversationPromptResponses { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<ModelTag> ModelTags { get; set; }
    public DbSet<Prompt> Prompts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<SystemMessage> SystemMessages { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ModelTag as a join entity
        modelBuilder.Entity<ModelTag>().HasKey(mt => new { mt.ModelId, mt.TagId });

        modelBuilder.Entity<ModelTag>()
            .HasOne(mt => mt.Model)
            .WithMany(m => m.ModelTags)
            .HasForeignKey(mt => mt.ModelId);

        modelBuilder.Entity<ModelTag>()
            .HasOne(mt => mt.Tag)
            .WithMany(t => t.ModelTags)
            .HasForeignKey(mt => mt.TagId);

        // Other entity configurations...
    }
}
```

## Dependency Injection Setup

```csharp
public static void ConfigureServices(IServiceCollection services)
{
    // Add DbContext
    services.AddDbContext<MyDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

    // Add Identity
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<MyDbContext>()
        .AddDefaultTokenProviders();

    // Register repositories
    services.AddScoped<IAIModelRepository, AIModelRepository>();
    services.AddScoped<IAIResponseRepository, AIResponseRepository>();
    services.AddScoped<IAttachmentRepository, AttachmentRepository>();
    services.AddScoped<IConversationPromptResponseRepository, ConversationPromptResponseRepository>();
    services.AddScoped<IConversationRepository, ConversationRepository>();
    services.AddScoped<IFeedbackRepository, FeedbackRepository>();
    services.AddScoped<IModelTagRepository, ModelTagRepository>();
    services.AddScoped<IPromptRepository, PromptRepository>();
    services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    services.AddScoped<ISystemMessageRepository, SystemMessageRepository>();
    services.AddScoped<ITagRepository, TagRepository>();
    services.AddScoped<IUserRepository, UserRepository>();

    // Register Unit of Work
    services.AddScoped<IUnitOfWork, UnitOfWork>();
}
```

## Usage Examples

### Repository Usage

```csharp
public class TagService
{
    private readonly IUnitOfWork _unitOfWork;

    public TagService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        return await _unitOfWork.Tags.GetAllAsync();
    }

    public async Task<Tag> GetTagByIdAsync(string id)
    {
        return await _unitOfWork.Tags.GetByIdAsync(id);
    }

    public async Task<bool> CreateTagAsync(Tag tag)
    {
        await _unitOfWork.Tags.AddAsync(tag);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateTagAsync(Tag tag)
    {
        await _unitOfWork.Tags.UpdateAsync(tag);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTagAsync(string id)
    {
        await _unitOfWork.Tags.SoftDeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
```

### Transaction Example

```csharp
public class ConversationService
{
    private readonly IUnitOfWork _unitOfWork;

    public ConversationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CreateConversationWithPromptAsync(Conversation conversation, Prompt prompt, AIResponse response)
    {
        // Add conversation
        await _unitOfWork.Conversations.AddAsync(conversation);
        
        // Add prompt
        await _unitOfWork.Prompts.AddAsync(prompt);
        
        // Add response
        await _unitOfWork.AIResponses.AddAsync(response);
        
        // Create conversation-prompt-response relationship
        var cpr = new ConversationPromptResponse
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conversation.Id,
            PromptId = prompt.Id,
            AIResponseId = response.Id,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        await _unitOfWork.ConversationPromptResponses.AddAsync(cpr);
        
        // Save all changes in a single transaction
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
}
```

These code examples demonstrate the key patterns and implementations used in the Ollama DB Layer, providing a reference for understanding and extending the codebase.