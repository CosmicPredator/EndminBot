using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using EndfieldBot.Helpers;

namespace EndfieldBot.Commands;

public class MiscCommandHandler(): ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("sysinfo", "Dump sysinfo")]
    public async Task HandleSysInfo()
    {
        if (Context.Interaction.User.Id != 793688107077468171)
        {
            var noAuthMessage = new InteractionMessageProperties()
                .WithContent("You are not authorized to use this command!")
                .WithFlags(MessageFlags.Ephemeral);
            await RespondAsync(InteractionCallback.Message(noAuthMessage));
            return;
        }

        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Loading));
        var latency = Context.Client.Latency.TotalMilliseconds;

        var response = new InteractionMessageProperties()
            .AddEmbeds(new EmbedProperties()
                .WithTitle("SysInfo for Endmin Bot")
                .WithColor(new Color(0x03fc39))
                .WithThumbnail(new EmbedThumbnailProperties("https://files.catbox.moe/4bswao.gif"))
                .WithFields([
                    new EmbedFieldProperties()
                        .WithName("Server Time")
                        .WithValue($"`{DateTime.Now.ToShortTimeString()}`")
                        .WithInline(true),
                    new EmbedFieldProperties()
                        .WithName("Latency")
                        .WithValue($"`{latency} ms`")
                        .WithInline(true),
                    new EmbedFieldProperties()
                        .WithName("Sys CPU")
                        .WithValue($"`{LinuxMetrics.GetSystemCpuUsage()} %`")
                        .WithInline(true),
                    new EmbedFieldProperties()
                        .WithName("Sys Mem")
                        .WithValue($"`{LinuxMetrics.GetSystemMemory().totalMb} MB`")
                        .WithInline(true),
                    new EmbedFieldProperties()
                        .WithName("Proc CPU")
                        .WithValue($"`{await LinuxMetrics.GetProcessCpuUsage()} %`")
                        .WithInline(true),
                    new EmbedFieldProperties()
                        .WithName("Proc Mem")
                        .WithValue($"`{LinuxMetrics.GetProcessMemoryMb()} MB`")
                        .WithInline(true),
                ]));
        await FollowupAsync(response);
    }
}