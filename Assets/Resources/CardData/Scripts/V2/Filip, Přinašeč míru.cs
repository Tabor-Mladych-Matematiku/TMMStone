using CardGame;

public class Filip__Přinašeč_míru : CardScriptBase
{
    protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e)
    {
        if (TryGetComponent(out Minion self) && self.Alive())
            GameManager.Instance.AddCardToHandByID(spell.Owner.Other(), spell.ID);
    }
}
