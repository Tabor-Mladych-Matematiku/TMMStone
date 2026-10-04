using CardGame;

public class Přímá_úměra : TargetableCardScriptBase
{
    protected override bool TargetValidate(TableActor target) => target is Minion;

    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        var mana = GameManager.Instance.ManaCounters[GetOwner(sender)];
        int damage = mana.Mana;
        mana.Mana = 0;
        DealSpellDamage((Minion)e.Target, damage, sender);
    }
}
