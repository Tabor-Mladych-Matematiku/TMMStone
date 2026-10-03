using CardGame;

public class Jiřík__Generátor_funfactů : CardScriptBase
{
    protected override void OnSpellPlayed(Card spell, Card.CardPlayedEventArgs e)
    {
        if (TryGetComponent(out Minion minion) && spell.Owner == minion.Owner && spell.ID == 33)
            GameManager.Instance.HPCounters[minion.Owner.Other()].Face.Damage(2);
    }
}
