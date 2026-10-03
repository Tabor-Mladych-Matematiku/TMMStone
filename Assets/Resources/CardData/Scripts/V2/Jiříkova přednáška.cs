using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Jiříkova_přednáška : CardScriptBase
{
    const int FunfactID = 33;
    protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) {
        GameManager.Instance.AddCardToHandByID(e.Player, FunfactID);
    }
}
