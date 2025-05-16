using Microsoft.Extensions.Hosting;
using Ollama_Component.Services.AuthService.Helpers;
using Ollama_Component.Services.ChatService.DTOs;
using Ollama_Component.Services.ChatService.Rag;
using Pinecone;
using Scalar.AspNetCore;
using System;

namespace Ollama_Component;

public class Program
{
    public static async Task Main(string[] args)
    {


        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();



        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        // Use Organized Service Registrations
        builder.Services.AddDatabaseAndIdentity(builder.Configuration);
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddRepositories();
        builder.Services.AddApplicationServices();
        builder.Services.ConfigureCors();
        builder.Services.ConfigureCache(builder.Configuration);
        builder.Services.ConfigureSwagger();



        builder.Services.Configure<RagOptions>(builder.Configuration.GetSection("RAG"));
        builder.Services.Configure<PineconeOptions>(builder.Configuration.GetSection("Pinecone"));


        builder.Services.AddSingleton<PineconeClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var apiKey = config["Pinecone:ApiKey"];
            var cloud = config["Pinecone:cloud"];
            var regin = config["Pinecone:region"];
            return new PineconeClient(apiKey);
        });



        builder.Services.AddSingleton<ITextEmbeddingGeneration>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var modelId = config["RAG:OllamaEmbeddingModelId"];
            var endpoint = config["RAG:OllamaEndpoint"];
            return new OllamaTextEmbeddingGeneration(modelId, endpoint);
        });



        builder.Services.AddScoped<IRagIndexingService, RagIndexingService>();
        builder.Services.AddScoped<IRagRetrievalService, RagRetrievalService>();



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
                    ConversationId = "test-conv-009",
                    Content = "what is the First Aid Supplies in Emergency Survival Kit?",
                    DocumentUrl = @"C:\Users\zigzag\Desktop\Example_Emergency_Survival_Kit.pdf"
                };

                // Index the document
                await indexer.IndexDocumentAsync(request);
                logger.LogInformation("Document indexed.");

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

        await app.RunAsync();




        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                options.DisplayRequestDuration();
            });

            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseMiddleware<TokenValidationMiddleware>(); 
        app.UseAuthorization();


        app.MapControllers();

        app.Run();

    }
}