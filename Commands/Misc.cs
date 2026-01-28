using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using EndfieldBot.Helpers;

namespace EndfieldBot.Commands;

public class MiscCommandHandler(ILogger<MiscCommandHandler> logger): ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("latency", "Checks the latency of bot")]
    public string HandleLatency()
    {
        var latency = Context.Client.Latency.TotalMilliseconds;
        return $"`Latency: {latency}ms`";
    }

    [SlashCommand("sysinfo", "Dump sysinfo")]
    public async Task<InteractionMessageProperties> HandleSysInfo()
    {
        if (Context.Interaction.User.Id != 793688107077468171)
        {
            return new InteractionMessageProperties()
                .WithContent("You are not authorized to use this command!")
                .WithFlags(MessageFlags.Ephemeral);
            
        }

        var latency = Context.Client.Latency.TotalMilliseconds;
    
        return new InteractionMessageProperties()
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
    }
}