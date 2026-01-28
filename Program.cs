using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddDiscordGateway(options =>
        {
            options.Intents = GatewayIntents.GuildMessages
                | GatewayIntents.DirectMessages
                | GatewayIntents.MessageContent
                | GatewayIntents.DirectMessageReactions
                | GatewayIntents.GuildMessageReactions;
        })
    .AddApplicationCommands();

var host = builder.Build();
host.AddModules(typeof(Program).Assembly);

await host.RunAsync();