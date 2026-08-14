using CrewmeleonRedrawn.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace CrewmeleonRedrawn.GameMode;

[RegisterInIl2Cpp]
public class ChameleonGameModeManager(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public static ChameleonGameModeManager? Instance { get; private set; }

    public static void Create()
    {
        if (Instance != null) return;

        new GameObject("ChameleonGameModeManager").AddComponent<ChameleonGameModeManager>();
    }

    public TimerStage CurrentStage => Timer.CurrentStage;

    private static bool CanUseChat => ChameleonOptions.Chat.ChatEnabled
                                      && (!ChameleonGameMode.AmImpostor || ChameleonOptions.Chat.SeekerCanSeeChat.Value);

    public readonly ChameleonTimer Timer = new();
    public readonly TauntTimer TauntTimer = new();
    public readonly PlayerTracker PlayerTracker = new();

    private int _deadPlayerCount;

    private void Awake()
    {
        Instance = this;

        try
        {
            ShipStatus.Instance.BreakEmergencyButton();
        }
        catch (Exception _)
        {
            Error("Could not find emergency button");
        }
        
        foreach (var player in Helpers.GetAlivePlayers())
            player.cosmetics.TogglePet(false);

        var hud = HudManager.Instance;
        hud.CrewmatesKilled.gameObject.SetActive(true);

        Timer.CreateTimer(hud);
        PlayerTracker.Begin(hud);
        
        Timer.SetStage(TimerStage.Hiding);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameHasStarted)
            return;

        HudUpdate(HudManager.Instance);
    }

    
    public void HudUpdate(HudManager instance)
    {
        if (!Timer.IsActive)
            return;

        // todo: maybe move this out into a HudManager patch instead of update looping
        instance.TaskStuff.gameObject.SetActive(false);
        instance.PetButton.gameObject.SetActive(false);
        instance.ReportButton.gameObject.SetActive(false);
        instance.SabotageButton.gameObject.SetActive(false);
        instance.ImpostorVentButton.gameObject.SetActive(false);
        instance.KillButton.gameObject.SetActive(false);
        instance.Chat.gameObject.SetActive(CanUseChat); 

        Timer.Update();
        PlayerTracker.Update();
        if (CurrentStage == TimerStage.Seeking) TauntTimer.Update();
    }

    public void NotifyOfDeath(PlayerControl player, bool infected = false)
    {
        _deadPlayerCount++;

        HudManager.Instance.NotifyOfDeath();

        var popupPrefab = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
        var popup = Instantiate(popupPrefab, HudManager.Instance.transform.parent);

        popup.text.GetComponent<TextTranslatorTMP>().DestroyImmediate();
        popup.text.text = infected ? "HAS BEEN INFECTED" : "HAS BEEN KILLED";
        popup.Show(player, _deadPlayerCount);
    }
    
    public float GetPlayerSpeed(PlayerControl pc) => pc.Data.Role is SeekerRole ? ChameleonOptions.Gameplay.SeekerSpeed.Value :
        CurrentStage == TimerStage.Hiding ? ChameleonOptions.Gameplay.SeekerSpeed.Value : ChameleonOptions.Gameplay.HiderSpeed.Value;
}
