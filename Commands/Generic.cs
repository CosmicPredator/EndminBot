using EndfieldBot.DB;
using EndfieldBot.Interfaces;
using EndfieldBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

namespace EndfieldBot.Commands;

public class GenericCommandHandler(
    IDbContextFactory<EndfieldBotDbContext> dbContextFactory,
    ILogger<GenericCommandHandler> logger,
    IConfiguration configuration
) : ApplicationCommandModule<ApplicationCommandContext>
{
    private EndfieldBotDbContext dbContext = dbContextFactory.CreateDbContext();

    [SlashCommand("code", "Latest redeem codes")]
    public async Task HandleCodeCommandAsync()
    {
        var codeFields = new List<EmbedFieldProperties>();
        foreach (var i in dbContext.RedeemCodes.AsNoTracking())
        {
            if (i.IsActive)
            {
                var description = i.Description is "?" ? "No Info" : i.Description;
                codeFields.Add(new EmbedFieldProperties()
                    .WithName($"`{i.Code}`")
                    .WithValue($"{description}"));
            }
        }
        var message = new InteractionMessageProperties()
            .AddEmbeds(new EmbedProperties()
                .WithTitle("Endfield Redeem Codes")
                .WithFields(codeFields)
                .WithThumbnail(
                    new EmbedThumbnailProperties("https://arknightsendfield.gg/wp-content/uploads/Endministrator-Build.webp"))
                .WithColor(new Color(0xae00ff))
            );

        await RespondAsync(InteractionCallback.Message(message));
    }

    [SlashCommand("events", "Ongoing events in Endfield")]
    public async Task HandleEventCommandAsync()
    {
        var events = dbContext.CurrentEvents.AsNoTracking();
        var msgBody = new EventEmbedBuilder(logger).BuildMessage(events.First(), 0, events.Count(), configuration);
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .AddEmbeds(msgBody.Embeds!)
            .AddComponents(msgBody.Components!)));
    }
}

public class EventComponentInteraction(
    IDbContextFactory<EndfieldBotDbContext> dbContextFactory,
    ILogger<EventComponentInteraction> logger,
    IConfiguration configuration
) : ComponentInteractionModule<ButtonInteractionContext>
{
    private EndfieldBotDbContext dbContext = dbContextFactory.CreateDbContext();

    [ComponentInteraction("event")]
    public async Task HandleEvent(int eventId)
    {
        var events = dbContext.CurrentEvents.AsNoTracking();
        if (!events.Any())
        {
            await RespondAsync(InteractionCallback.ModifyMessage(message =>
            {
                message.Content = "No Events Available";
                message.Flags = MessageFlags.Ephemeral;
            }));
            return;
        }
        var efEvent = events.ElementAt(eventId);
        await RespondAsync(InteractionCallback.ModifyMessage(message =>
        {
            var msgBody = new EventEmbedBuilder(logger).BuildMessage(efEvent, eventId, events.Count(), configuration);
            message.AddEmbeds(msgBody.Embeds!);
            message.AddComponents(msgBody.Components!);
        }));
    }
}

public class MessageBody
{
    public IMessageComponentProperties? Components { get; set; }
    public EmbedProperties? Embeds { get; set; }
}

public class EventEmbedBuilder(ILogger logger)
{
    public MessageBody BuildMessage(CurrentEvents efEvent, int index, int maxSize, IConfiguration config)
    {
        var baseHost = config["Endfield:BaseHost"]!;
        var actionRow = new ActionRowProperties()
            .WithId(new Random().Next());
        if (index > 0)
        {
            actionRow.AddComponents(
                new ButtonProperties($"event:{index - 1}", "⬅️", ButtonStyle.Secondary)
            );
        }

        if (!Uri.TryCreate(efEvent.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            uri = new Uri(new Uri(baseHost), efEvent.Url);
        }
        
        actionRow.AddComponents(
            new LinkButtonProperties(uri.ToString(), "Event Link")
        );

        logger.LogInformation("CUURENT INDEX: {}", index);
        logger.LogInformation("TOTAL COUNT: {}", maxSize);
        if (index < maxSize - 1)
        {
            actionRow.AddComponents(
                new ButtonProperties($"event:{index + 1}", "➡️", ButtonStyle.Secondary)
            );
        }
        var body = new MessageBody();
        body.Embeds = new EmbedProperties()
            .WithImage(efEvent.CoverImage)
            .WithTitle(efEvent.Name)
            .WithDescription(efEvent.Description)
            .AddFields(new EmbedFieldProperties()
                .WithName("Start Date")
                .WithValue(efEvent.StartDate.ToString())
                .WithInline(true))
            .AddFields(new EmbedFieldProperties()
                .WithName("End Date")
                .WithValue(efEvent.EndDate.ToString())
                .WithInline(true));

        body.Components = actionRow;

        return body;
    }
}