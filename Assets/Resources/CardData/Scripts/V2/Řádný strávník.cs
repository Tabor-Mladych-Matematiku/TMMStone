using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Řádný_strávník : CardScriptBase
{
    //protected override void OnDiscard(object sender, EventArgs e){}

    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}

    //protected override void OnStartTurn(object sender, GameActor.TurnEventArgs e){}
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        ForEachPlayerDo(
            (player) => GameManager.Instance.HPCounters[player].Heal(4),
            sender
            );
    }
}