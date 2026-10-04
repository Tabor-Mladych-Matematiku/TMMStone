using System.Linq;
using CardGame;

public class EVIL_Jiřík__Pán_chaosu : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager manager = GameManager.Instance;
        GameManager.P first = GetOwner(sender);
        Deck firstDeck = manager.decks[first];
        Deck secondDeck = manager.decks[first.Other()];
        if (ReferenceEquals(firstDeck.Pile, secondDeck.Pile)) return;

        CardPile shared = new(firstDeck.Concat(secondDeck));
        shared.Cards.Shuffle();
        firstDeck.UsePile(shared);
        secondDeck.UsePile(shared);
    }
}
