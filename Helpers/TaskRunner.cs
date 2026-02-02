using System.Collections.Concurrent;
using EndfieldBot.DB;
using EndfieldBot.Interfaces;
using EndfieldBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EndfieldBot.Helpers;

public class TaskRunner : BackgroundService
{
    private EndfieldBotDbContext dbContext;
    private readonly IRequestHandler requestHandler;
    private readonly ILogger<TaskRunner> logger;
    private readonly IConfiguration config;
    private readonly TaskQueue taskQueue;

    public TaskRunner(
        IRequestHandler handler,
        IDbContextFactory<EndfieldBotDbContext> dbFactory,
        ILogger<TaskRunner> logger,
        IConfiguration config,
        TaskQueue taskQueue)
    {
        this.requestHandler = handler;
        this.logger = logger;
        this.dbContext = dbFactory.CreateDbContext();
        this.taskQueue = taskQueue;
        this.config = config;
    }

    private async Task syncCodesEvents()
    {
        var url = config.GetValue<string>("Endfield:CodeEventUrl")!;
        var result = await requestHandler.GetAsync<EfHomeModel>(url);
        if (result is null)
        {
            logger.LogError("Failed to query host with URL: {}", url);
            return;
        }

        await using var tx = await dbContext.Database.BeginTransactionAsync();
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM RedeemCodes");
        await tx.CommitAsync();

        await dbContext.RedeemCodes.AddRangeAsync(result.Codes.Select(x => x.ToEntity()));
        await dbContext.CurrentEvents.AddRangeAsync(result.Events.Select(x => x.ToEntity()));
        await dbContext.SaveChangesAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Starting task processor");
        while (!stoppingToken.IsCancellationRequested)
        {
            var item = await taskQueue.DequeueAsync(stoppingToken);
            if (item is null)
                continue;

            switch (item.Type)
            {
                case TaskType.RefreshCodeEvent:
                    logger.LogDebug("Processing {} task. Executing now", item.Type);
                    await syncCodesEvents();
                    break;
            }
        }
    }
}

public class TaskQueue
{
    private readonly ConcurrentQueue<TaskQueueItem> taskQueue = new();
    private readonly SemaphoreSlim signal = new(0);

    public void Enqueue(TaskQueueItem item)
    {
        taskQueue.Enqueue(item);
        signal.Release();
    }

    public async Task<TaskQueueItem?> DequeueAsync(CancellationToken token)
    {
        await signal.WaitAsync(token);
        taskQueue.TryDequeue(out var item);
        return item;
    }
}

public class TaskQueueItem
{
    public TaskType Type { get; set; }
    public Dictionary<string, string>? Params { get; set; }
}

public enum TaskType
{
    RefreshCodeEvent,
    NullEvent
}