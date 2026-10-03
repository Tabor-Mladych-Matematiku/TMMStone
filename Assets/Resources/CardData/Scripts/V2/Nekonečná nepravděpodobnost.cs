using CardGame;

public class Nekonečná_nepravděpodobnost : TargetableCardScriptBase
{
    protected override bool TargetValidate(TableActor target) => target is DamageableActor;

    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        DamageableActor target = (DamageableActor)e.Target;
        bool heal = RandomRange(0, 2) == 1;
        int amount = RandomRange(1, 7);
        if (heal) target.Heal(amount);
        else DealSpellDamage(target, amount, sender);
    }
}
