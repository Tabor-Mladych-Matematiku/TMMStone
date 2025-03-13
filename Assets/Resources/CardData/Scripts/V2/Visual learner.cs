using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Visual_learner : CardScriptBase
{
    protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e)
    {
        if (TryGetComponent(out Minion minion) && minion.Owner != spell.Owner)
        {
            minion.Buff(1, 0);
        }
    }
}