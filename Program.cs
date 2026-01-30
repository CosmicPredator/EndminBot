using EndfieldBot.Commands;
using EndfieldBot.Helpers;
using EndfieldBot.Interfaces;
using EndfieldBot.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddHttpClient<IRequestHandler, RequestHandler>();

builder.Services
    .AddSingleton<SimpleCache>()
    .AddDiscordGateway(options =>
        {
            options.Intents = GatewayIntents.GuildMessages
                | GatewayIntents.DirectMessages
                | GatewayIntents.MessageContent
                | GatewayIntents.DirectMessageReactions
                | GatewayIntents.GuildMessageReactions;
        })
    .AddApplicationCommands()
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>();

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

await host.RunAsync();

public class SimpleCache
{
    public IReadOnlyList<EfHomeModelEvent>? Events {get; set;}
    public IReadOnlyList<EfHomeModelCode>? Codes {get; set;}
}


