using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Michalův_matematický_model : CardScriptBase
{
    protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e) {
        Minion minion = gameObject.GetComponent<Minion>();
        if (minion == null || minion.Owner != spell.Owner) return;
        GameManager.Instance.decks[minion.Owner].AddRandom(spell);
    }
}