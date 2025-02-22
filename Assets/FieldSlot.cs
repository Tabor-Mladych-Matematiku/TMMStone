using CardGame;
using System.Collections;
using System.Collections.Generic;

public class FieldSlot : PlacableSlot
{
    public override bool IsCardPlacable(Card c) => c.cardType == Card.CardType.Field;
    public Field GetField()=>GetComponentInChildren<Field>();

    public void ClearField()
    {
        Field field = GetField();
        if(field != null) { 
            Destroy(transform.GetComponent<Field>());
            GameManager.Instance.AddToGrave(PopCard(), Owner);
        }
        
    }
}
