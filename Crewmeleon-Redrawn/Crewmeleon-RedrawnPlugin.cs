using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace Crewmeleon_Redrawn;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class CrewmeleonRedrawnPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        Harmony.PatchAll();
        ReactorCredits.Register<CrewmeleonRedrawnPlugin>(location => location is ReactorCredits.Location.MainMenu);
    }

    public override bool Unload()
    {
        Harmony.UnpatchSelf();

        return base.Unload();
    }
}
