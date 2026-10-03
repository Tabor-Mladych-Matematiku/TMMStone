using CardGame;

public class Příchod_Zimnice : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is not DamageableActor target) return;
        if (target.Frozen) DealSpellDamage(target, 4, sender);
        else target.Frozen = true;
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
