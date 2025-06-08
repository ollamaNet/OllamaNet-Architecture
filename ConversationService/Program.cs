using ConversationService;
using ConversationService.Services.Rag.Interfaces;
using ConversationServices;
using ConversationServices.Services.ChatService.DTOs;
using Pinecone;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Use Organized Service Registrations
builder.Services.AddDatabaseAndIdentity(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.ConfigureCors(builder.Configuration);
builder.Services.ConfigureCache(builder.Configuration);
builder.Services.ConfigureSwagger(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation(" Resolving services...");

        var indexer = services.GetRequiredService<IRagIndexingService>();
        var retriever = services.GetRequiredService<IRagRetrievalService>();

        logger.LogInformation(" Services resolved.");

        var request = new PromptRequest
        {
            ConversationId = "test-conv-012",
            Content = "what is the First Aid Supplies in Emergency Survival Kit?",
            DocumentUrl = @"C:\Users\zigzag\Desktop\Example_Emergency_Survival_Kit.pdf"
        };

        // Index the document
        await indexer.IndexDocumentAsync(request);
        logger.LogInformation(" Document indexed.");

        // Retrieve relevant chunks
        var contextChunks = await retriever.GetRelevantContextAsync(request);

        if (contextChunks?.Any() == true)
        {
            var retrievedContext = string.Join("\n---\n", contextChunks);
            var systemContext = $"Use the following retrieved context from the user's uploaded document to answer the query:\n{retrievedContext}";

            // Simulated chat history object
            var history = new List<string> { systemContext };

            logger.LogInformation(" RAG Retrieved Context:\n{Context}", systemContext);
        }
        else
        {
            logger.LogWarning(" No relevant context chunks found.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, " An error occurred during RAG processing.");
    }
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var version = builder.Configuration["Swagger:Version"] ?? "v1";
        var title = builder.Configuration["Swagger:Title"] ?? "API";
        options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{title} {version}");
        options.DisplayRequestDuration();
    });
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
