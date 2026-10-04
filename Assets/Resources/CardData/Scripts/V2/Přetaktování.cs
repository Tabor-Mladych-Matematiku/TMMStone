using CardGame;
using CardData;

public class Přetaktování : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is not Minion minion) return;
        DealSpellDamage(minion, 1, sender);
        if (minion.Health > 0) minion.Buff(2, 0);
    }

    protected override bool TargetValidate(TableActor target) =>
        target is Minion minion && minion.HasTag(CardTag.Stroj);
}
