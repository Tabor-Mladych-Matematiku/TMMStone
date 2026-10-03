using CardGame;

public class Duch_matemágův : CardScriptBase
{
    protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e)
    {
        Minion self = GetComponent<Minion>();
        if (spell.Owner == self.Owner) self.Buff(2, 2);
    }
}
