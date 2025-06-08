using ConversationServices;


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

    //app.MapScalarApiReference();
}
app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
