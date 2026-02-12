using System.Diagnostics;
using NetCord;
using NetCord.Gateway.Voice;
using NetCord.Logging;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EndfieldBot.Commands;

public class VoiceCommandsHandler: ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("play", "plays the song using mpv", Contexts = [InteractionContextType.Guild])]
    public async Task HandlePlayAsync(string track)
    {
        // if (!Uri.IsWellFormedUriString(track, UriKind.Absolute))
        // {
        //     await RespondAsync(InteractionCallback.Message("Invalid track!"));
        //     return;
        // }

        var guild = Context.Guild!;

        if (!guild.VoiceStates.TryGetValue(Context.User.Id, out var voiceState))
        {
            await RespondAsync(InteractionCallback.Message("You are not connected to any voice channel!"));
            return;
        }

        var client = Context.Client;

        var voiceClient = await client.JoinVoiceChannelAsync(
            guild.Id,
            voiceState.ChannelId.GetValueOrDefault(),
            new VoiceClientConfiguration
            {
                Logger = new ConsoleLogger(),
            });

        await voiceClient.StartAsync();
        await voiceClient.EnterSpeakingStateAsync(new SpeakingProperties(SpeakingFlags.Microphone));
        await RespondAsync(InteractionCallback.Message($"Playing {Path.GetFileName(track)}!"));

        var voiceStream = voiceClient.CreateVoiceStream();
        OpusEncodeStream stream = new(voiceStream, PcmFormat.Short, VoiceChannels.Stereo, OpusApplication.Audio);

        ProcessStartInfo startInfo = new("ffmpeg")
        {
            RedirectStandardOutput = true,
        };
        var arguments = startInfo.ArgumentList;

        arguments.Add("-reconnect");
        arguments.Add("1");

        arguments.Add("-reconnect_streamed");
        arguments.Add("1");

        arguments.Add("-reconnect_delay_max");
        arguments.Add("5");

        arguments.Add("-i");
        arguments.Add(track);

        arguments.Add("-loglevel");
        arguments.Add("-8");

        arguments.Add("-ac");
        arguments.Add("2");

        arguments.Add("-f");
        arguments.Add("s16le");

        arguments.Add("-ar");
        arguments.Add("48000");

        arguments.Add("pipe:1");

        var ffmpeg = Process.Start(startInfo)!;
        await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);

        await stream.FlushAsync();
    }
}
