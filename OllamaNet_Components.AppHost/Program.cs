var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Ollama_Component>("ollama-component");

builder.Build().Run();
