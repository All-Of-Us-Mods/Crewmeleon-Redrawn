using System.Runtime.CompilerServices;
using CrewmeleonRedrawn.Roles;
using PerfectComms.Api;

namespace CrewmeleonRedrawn.Integrations;

internal static class PerfectCommsIntegration
{
    private const string PluginId = "com.edgetel.perfectcomms";

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Register()
    {
        PerfectCommsApi.RegisterVoiceChannel(CrewmeleonRedrawnPlugin.Id, ctx =>
            new VoiceChannelResult("everyone", Shape: VoiceAudioShape.Radio));

        PerfectCommsApi.RegisterVoiceChannel(CrewmeleonRedrawnPlugin.Id, ctx => ctx.Player.Data.Role switch
        {
            HiderRole => new VoiceChannelResult("hiders", Shape: VoiceAudioShape.Radio),
            SeekerRole => new VoiceChannelResult("seekers", Shape: VoiceAudioShape.Radio),
            _ => null
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool IsLoaded()
        => BepInEx.Unity.IL2CPP.IL2CPPChainloader.Instance.Plugins.ContainsKey(PluginId);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Unregister()
        => PerfectCommsApi.Unregister(CrewmeleonRedrawnPlugin.Id);
}