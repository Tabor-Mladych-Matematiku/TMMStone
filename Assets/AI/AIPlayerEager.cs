using CardGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerEager : AIPlayerBase
{
    public override void OnTurnStart()
    {
        base.OnTurnStart();
        Debug.Log("AIPlayerEager.OnTurnStart.");


        Debug.Log("Ending turn.");
        GameManager.Instance.ToggleTurnOffline();

    }
}
