using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using DevTeamAI.LLM.Services;
using DevTeamAI;

var builder = Host.CreateApplicationBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Register LLM services and application
builder.Services.AddLLMServices(builder.Configuration);
builder.Services.AddTransient<DevTeamAIApplication>();

var host = builder.Build();

// Run the application
await host.Services.GetRequiredService<DevTeamAIApplication>().RunAsync();
