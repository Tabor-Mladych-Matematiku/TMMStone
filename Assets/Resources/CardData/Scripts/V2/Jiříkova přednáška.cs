using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Jiříkova_přednáška : CardScriptBase
{
    protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) {
        GameManager.Instance.DrawCard(GameManager.Instance.PlayerOnTurn);
    }
}