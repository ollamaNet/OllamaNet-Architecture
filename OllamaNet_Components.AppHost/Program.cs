var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Ollama_Component>("ollama-component");

builder.AddProject<Projects.Gateway>("gateway");

builder.AddProject<Projects.Admin_Component>("admin-component");

builder.Build().Run();
