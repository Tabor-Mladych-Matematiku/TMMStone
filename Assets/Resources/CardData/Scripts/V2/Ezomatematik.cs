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
    protected override void OnCardPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (sender is Card card &&
            card.cardType == Card.CardType.Spell &&
            !gameObject.TryGetComponent(out Minion minion)
            ) GameManager.Instance.HPCounters[minion.Owner].Heal(2);
    }
}