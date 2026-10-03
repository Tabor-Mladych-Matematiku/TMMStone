using CardGame;

public class Statická_elektřina : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is DamageableActor target) DealSpellDamage(target, 1, sender);
        DrawCard(true, sender);
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
