using System.Globalization;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using GothmogBot;
using GothmogBot.Discord;
using GothmogBot.Services;
using GothmogBot.Stratz;
using Serilog;

IConfiguration configuration = new ConfigurationBuilder()
	.SetBasePath(Environment.CurrentDirectory)
	.AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
	.AddEnvironmentVariables()
	.Build();

// Create builder
var builder = WebApplication.CreateBuilder(args);

// Add options
builder.Services
	.AddOptions<DiscordOptions>()
	.Bind(configuration.GetSection(DiscordOptions.SectionName))
	.Validate(o => !string.IsNullOrEmpty(o.DiscordApiToken), "DiscordApiToken must have a value.");

builder.Services
	.AddOptions<StratzOptions>()
	.Bind(configuration.GetSection(StratzOptions.SectionName))
	.Validate(o => !string.IsNullOrEmpty(o.StratzApiToken), "StratzApiToken must have a value.");

var stratzOptions = configuration
	.GetSection(StratzOptions.SectionName)
	.Get<StratzOptions>() ?? throw new InvalidOperationException("No StratzOptions provided.");

// Add HttpClient
builder.Services.AddHttpClient();

// Add Stratz GraphQL Client
builder.Services.AddStratzGraphQLClient(stratzOptions.StratzApiToken);

// Add serilog
builder.Host.UseSerilog();
builder.Logging.AddSerilog();

var loggerConfiguration = new LoggerConfiguration()
	.WriteTo.Console(formatProvider: CultureInfo.CurrentCulture)
	.Enrich.FromLogContext();

Log.Logger = loggerConfiguration.CreateLogger();

// Add Discord Services
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<DiscordRestClient>();

// Add local services
builder.Services.AddSingleton<IDiscordClientRunner, DiscordClientRunner>();
builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<InteractionService>(services => new InteractionService(services.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<InteractionHandler>();
builder.Services.AddSingleton<DotaService>();

// Build and run app
var app = builder.Build();

var discordRunner = app.Services.GetRequiredService<IDiscordClientRunner>();

var appRunTask = app.RunAsync();
var discordRunnerTask = discordRunner.RunAsync();

await Task.WhenAll(discordRunnerTask, appRunTask).ConfigureAwait(false);
