using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.UI;
using HarmonyLib;
using MiraAPI;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using ReactUI.Plugin;

namespace Crewmeleon_Redrawn;

[BepInAutoPlugin("dev.allofus.crewmeleon", "Crewmeleon: Redrawn", "1.0.0")]
[BepInProcess("Among Us.exe")]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(ReactUIPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class CrewmeleonRedrawnPlugin : BasePlugin, IMiraPlugin
{

    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        Harmony.PatchAll();
        ReactorCredits.Register<CrewmeleonRedrawnPlugin>(location => location is ReactorCredits.Location.MainMenu);

        CrewmeleonStyles.Register();
        ReactUI.UI.Render(BrushPanel.Render);
        ReactUI.Plugin.ReactUIBehaviour.OnUpdate += BrushPanel.Tick;
    }

    public override bool Unload()
    {
        Harmony.UnpatchSelf();

        return base.Unload();
    }

    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public string OptionsTitleText => "Crewmeleon: Redrawn";
}
