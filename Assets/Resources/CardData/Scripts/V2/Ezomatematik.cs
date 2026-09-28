using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Ezomatematik : CardScriptBase
{
    //protected override void OnDiscard(object sender, EventArgs e){}

    //protected override void OnEndTurn(object sender, GameActor.TurnEventArgs e){}

    //protected override void OnStartTurn(object sender, GameActor.TurnEventArgs e){}

    //protected override void OnBattleCry(){}
    protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e)
    {
        if (spell.Owner == GameManager.Instance.PlayerOnTurn&&
            gameObject.TryGetComponent(out Minion minion))
        {
            GameManager.Instance.HPCounters[minion.Owner].Heal(2);
        }
    }
}