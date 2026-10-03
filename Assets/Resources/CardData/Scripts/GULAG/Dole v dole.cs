
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Dole_v_dole : CardScriptBase
{
    const int UranID = 239;
    protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) {
        GameManager.P player = e.Player;
        GameManager.Instance.HPCounters[player].Face.Damage(2);
        GameManager.Instance.AddCardToHandByID(player, UranID);
    }

}
