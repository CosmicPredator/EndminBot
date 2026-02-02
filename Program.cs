using EndfieldBot.Commands;
using EndfieldBot.DB;
using EndfieldBot.Helpers;
using EndfieldBot.Interfaces;
using EndfieldBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    .AddDbContextFactory<EndfieldBotDbContext>(options =>
    {
        options.UseSqlite(
            builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db");
    })
    .AddDiscordGateway(options =>
        {
            options.Intents = GatewayIntents.GuildMessages
                | GatewayIntents.DirectMessages
                | GatewayIntents.MessageContent
                | GatewayIntents.DirectMessageReactions
                | GatewayIntents.GuildMessageReactions;
        })
    .AddApplicationCommands()
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
    .AddSingleton<TaskQueue>()
    .AddHostedService<TaskRunner>();

var host = builder.Build();
host.AddModules(typeof(Program).Assembly);

using(var scope = host.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IDbContextFactory<EndfieldBotDbContext>>();
    using var db = await dbContext.CreateDbContextAsync();
    await db.Database.MigrateAsync();

    // var taskRunner = scope.ServiceProvider.GetRequiredService<TaskQueue>();
    // taskRunner.Enqueue(new TaskQueueItem()
    // {
    //     Type = TaskType.RefreshCodeEvent,
    //     Params = null
    // });
}

await host.RunAsync();

