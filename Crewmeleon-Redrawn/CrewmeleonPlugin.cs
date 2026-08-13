using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using CrewmeleonRedrawn.UI;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using ReactUI.Plugin;
using MiraAPI;
using MiraAPI.PluginLoading;

namespace CrewmeleonRedrawn;

[BepInAutoPlugin("dev.allofus.crewmeleon", "Crewmeleon: Redrawn")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class CrewmeleonRedrawnPlugin : BasePlugin, IMiraPlugin
{
    public Harmony Harmony { get; } = new(Id);

    public string OptionsTitleText => Name;

    public override void Load()
    {
        Harmony.PatchAll();

        ReactorCredits.Register<CrewmeleonRedrawnPlugin>(location => location is ReactorCredits.Location.MainMenu);

        ReactUIBootstrap.Initialize();
        CrewmeleonStyles.Register();
        BrushPanel.Mount();
        ReactUIBehaviour.OnUpdate += BrushPanel.Tick;
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
}
