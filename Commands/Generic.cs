using EndfieldBot.Interfaces;
using EndfieldBot.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EndfieldBot.Commands;

public class GenericCommandHandler(
    IRequestHandler requestHandler
) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("code", "Latest redeem codes")]
    public async Task HandleCodeAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var url = "https://endfieldtools.dev/localdb/home-page.json";
        var response = await requestHandler.GetAsync<EfHomeModel>(url);

        if (response is null)
        {
            await FollowupAsync(new InteractionMessageProperties()
                .WithContent("Request to Endfield Database failed!")
                .WithFlags(MessageFlags.Ephemeral));
            return;
        }

        var codeFields = new List<EmbedFieldProperties>();
        foreach (var i in response.Codes)
        {
            if (i.Active)
            {
                var description = i.Description is "?" ? "No Info": i.Description;
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
}