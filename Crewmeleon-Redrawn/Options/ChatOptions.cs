using CrewmeleonRedrawn.GameMode;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace CrewmeleonRedrawn;

public class ChatOptions : AbstractOptionGroup<ChameleonGameMode>
{
    public override string GroupName => "Chat Options";
    public override uint GroupPriority => 2;
    public ModdedToggleOption ChatEnabled { get; } =
        new ModdedToggleOption("Chat Enabled", true);
    public ModdedToggleOption SeekerCanSeeChat { get; } =
        new ModdedToggleOption("Seeker Can See Chat", true)
        {
            Visible = () => OptionGroupSingleton<ChatOptions>.Instance.ChatEnabled.Value,
        };
}