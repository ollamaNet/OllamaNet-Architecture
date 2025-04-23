var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Ollama_Component>("ollama-component");

builder.AddProject<Projects.Gateway>("gateway");

builder.AddProject<Projects.ExploreService>("exploreservice");

builder.AddProject<Projects.AuthService>("authservice");

builder.AddProject<Projects.AdminService>("adminservice");

builder.AddProject<Projects.ConversationService>("conversationservice");

builder.Build().Run();
