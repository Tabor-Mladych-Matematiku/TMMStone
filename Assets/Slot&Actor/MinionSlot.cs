using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CardGame
{
    public class MinionSlot : PlacableSlot
    {
        public override bool IsCardPlacable(Card c) => c.cardType == Card.CardType.Minion;
        protected override void OnMouseOver()
        {
            if (Owner != GameManager.P.P1) return;
            base.OnMouseOver();
        }
    }
}