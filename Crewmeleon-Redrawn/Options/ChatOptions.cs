using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using Reactor.Utilities;

namespace Crewmeleon_Redrawn;

public class ChatOptions : AbstractOptionGroup
{
    public override string GroupName => "Chat Options";

    public ModdedToggleOption ChatEnabled { get; } =
        new ModdedToggleOption("Chat Enabled", true);
    public ModdedToggleOption SeekerCanSeeChat { get; } =
        new ModdedToggleOption("Seeker Can See Chat", true)
        {
            Visible = (() => OptionGroupSingleton<ChatOptions>.Instance.SeekerCanSeeChat.Value),
        };
}