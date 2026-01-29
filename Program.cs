using EndfieldBot.Helpers;
using EndfieldBot.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddHttpClient<IRequestHandler, RequestHandler>();

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