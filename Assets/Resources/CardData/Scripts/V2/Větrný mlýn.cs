using System;
using System.Linq;
using CardGame;

public class Větrný_mlýn : CardScriptBase
{
    protected override Func<Card, int> ManaCostModifier => card =>
        -GameManager.Instance.GetCardsInHandOwnedBy(card.Owner).Count(other => other != card);
}
