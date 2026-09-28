using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;
using System.Linq;

public class Recyklovaná_přednáška : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.P owner = GetOwner(sender);
        Grave grave = GameManager.Instance.graves[owner];
        Card spell = grave.LastOrDefault(card => card.cardType == Card.CardType.Spell);
        if (spell == null || !grave.Take(spell)) return;

        GameManager.Instance.AddCardToHand(owner, spell, true);
    }
}
