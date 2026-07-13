using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CardGame
{
    public class FieldSlot : PlacableSlot
    {
        public override bool IsCardPlacable(Card c) => c.cardType == Card.CardType.Field;
        public Field GetField() => GetComponentInChildren<Field>();

        public void ClearField()
        {
            Field field = GetField();
            if (field != null)
            {
                field.transform.SetParent(null);
                Destroy(field.gameObject);
                GameManager.Instance.AddToGrave(PopCard(), Owner);
            }
        }
    }
}