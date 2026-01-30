using EndfieldBot.Interfaces;
using EndfieldBot.Models;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

namespace EndfieldBot.Commands;

public class GenericCommandHandler(
    IRequestHandler requestHandler,
    SimpleCache cache,
    ILogger<GenericCommandHandler> logger
) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("code", "Latest redeem codes")]
    public async Task HandleCodeCommandAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        if (cache.Codes is null)
        {
            logger.LogInformation("Cache not found, performing HTTP request");
            var url = "https://endfieldtools.dev/localdb/home-page.json";
            var request = await requestHandler.GetAsync<EfHomeModel>(url);

            if (request is null)
            {
                await FollowupAsync(new InteractionMessageProperties()
                    .WithContent("Request to Endfield Database failed!")
                    .WithFlags(MessageFlags.Ephemeral));
                return;
            }
            cache.Codes = request.Codes;
            cache.Events = request.Events;
        }

        var codeFields = new List<EmbedFieldProperties>();
        foreach (var i in cache.Codes)
        {
            if (i.Active)
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

        await FollowupAsync(message);
    }

    [SlashCommand("events", "Ongoing events in Endfield")]
    public async Task HandleEventCommandAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        if (cache.Events is null)
        {
            logger.LogInformation("Cache not found, performing HTTP request");
            var url = "https://endfieldtools.dev/localdb/home-page.json";
            var request = await requestHandler.GetAsync<EfHomeModel>(url);

            if (request is null)
            {
                await FollowupAsync(new InteractionMessageProperties()
                    .WithContent("Request to Endfield Database failed!")
                    .WithFlags(MessageFlags.Ephemeral));
                return;
            }
            cache.Codes = request.Codes;
            cache.Events = request.Events;
        }

        var msgBody = new EventEmbedBuilder(logger).BuildMessage(cache.Events[0], 0, cache.Events.Count);
        await FollowupAsync(new InteractionMessageProperties()
            .AddEmbeds(msgBody.Embeds!)
            .AddComponents(msgBody.Components!));
    }
}

public class EventComponentInteraction(
    SimpleCache cache,
    ILogger<EventComponentInteraction> logger
) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("event")]
    public async Task HandleEvent(int eventId)
    {
        if (cache.Events is null)
        {
            await RespondAsync(InteractionCallback.ModifyMessage(message =>
            {
                message.Content = "No Events Available";
                message.Flags = MessageFlags.Ephemeral;
            }));
            return;
        }
        var efEvent = cache.Events[eventId];

        await RespondAsync(InteractionCallback.ModifyMessage(message =>
        {
            var msgBody = new EventEmbedBuilder(logger).BuildMessage(efEvent, eventId, cache.Events.Count);
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
    public MessageBody BuildMessage(EfHomeModelEvent efEvent, int index, int maxSize)
    {
        var baseHost = "https://endfieldtools.dev";
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