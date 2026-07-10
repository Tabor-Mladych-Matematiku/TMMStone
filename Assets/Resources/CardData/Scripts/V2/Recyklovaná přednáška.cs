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
        GameManager.Instance.AddCardToHand(owner, grave.Where(card => card.cardType == Card.CardType.Spell).Last());
        /*for (int i = grave.Count-1; i >=0 ; i--)
        {
            Card card = grave[i];
            if (card.cardType == Card.CardType.Spell)
            {
                grave.RemoveAt(i);//Does nuffin' rn since the AddCardToHand takes it away anyway
                GameManager.Instance.AddCardToHand(owner, card);
                return;
            }
        }*/


    }
}