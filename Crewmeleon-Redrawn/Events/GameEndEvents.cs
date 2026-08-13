using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using UnityEngine;

namespace CrewmeleonRedrawn.Events;

public class GameEndEvents
{
    //Resets the cursor sprite, stops the cursor from keeping its look in lobby (For example, for when shooting with the shotgun)
    //Kind of a band aid solution imo
    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent e)
    {
        Cursor.SetCursor(null, CursorMode.Auto);
    }
}