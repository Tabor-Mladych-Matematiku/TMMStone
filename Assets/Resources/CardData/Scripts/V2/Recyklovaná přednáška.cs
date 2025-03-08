using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Recyklovaná_přednáška : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) {
        GameManager.P owner = GetOwner(sender);
        Grave grave = GameManager.Instance.graves[owner];
        for (int i = grave.Count - 1; i >= 0; i--)
        {
            Card card = grave[i];
            if(card.cardType == Card.CardType.Spell)
            {
                grave.RemoveAt(i);//Does nuffin' rn since the AddCardToHand takes it away anyway
                GameManager.Instance.AddCardToHand(owner, card);
                return;
            }
        }
        
        
    }
}