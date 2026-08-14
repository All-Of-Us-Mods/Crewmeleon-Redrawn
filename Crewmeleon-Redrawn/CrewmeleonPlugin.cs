global using static Reactor.Utilities.Logger<CrewmeleonRedrawn.CrewmeleonRedrawnPlugin>;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using CrewmeleonRedrawn.States;
using CrewmeleonRedrawn.UI;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using ReactUI.Plugin;
using MiraAPI;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using CrewmeleonRedrawn.Integrations;

namespace CrewmeleonRedrawn;

[BepInAutoPlugin("dev.allofus.crewmeleon", "Crewmeleon: Redrawn")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency("com.edgetel.perfectcomms", BepInDependency.DependencyFlags.SoftDependency)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class CrewmeleonRedrawnPlugin : BasePlugin, IMiraPlugin
{
    public Harmony Harmony { get; } = new(Id);

    public bool DisplayOnOptionsMenu => false;
    public string OptionsTitleText => Name;

    public static bool IsMobile;

    public override void Load()
    {
        Harmony.PatchAll();
        IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS(); //iOS :heh:
        ReactorCredits.Register<CrewmeleonRedrawnPlugin>(ReactorCredits.AlwaysShow);

        ReactUIBootstrap.Initialize();
        CrewmeleonStyles.Register();
        BrushPanel.Mount();
        ReactUIBehaviour.OnUpdate += BrushPanel.Tick;
        
        SceneManager.activeSceneChanged += (UnityAction<Scene, Scene>) new Action<Scene, Scene>((s1, s2) =>
        {
            Log.LogInfo($"Scene changed from {s1} to {s2}, Resetting cursor...");
            Cursor.SetCursor(null, CursorMode.Auto);
            StateManager.ClearAll();
        });
        if (PerfectCommsIntegration.IsLoaded())
            PerfectCommsIntegration.Register();
    }

    public override bool Unload()
    {
        Harmony.UnpatchSelf();

        if (PerfectCommsIntegration.IsLoaded())
            PerfectCommsIntegration.Unregister();
 

        return base.Unload();
    }

    public ConfigFile GetConfigFile()
    {
        return Config;
    }
}
