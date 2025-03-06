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
    protected override void OnPlayed(object sender, Card.CardPlayedEventArgs e){
        if (sender is Card card && card.cardType != Card.CardType.Spell) return;
        Minion minion = gameObject.GetComponent<Minion>();
        if(minion == null) return;
        GameManager.Instance.HPCounters[minion.Owner].Heal(2);
    }
}