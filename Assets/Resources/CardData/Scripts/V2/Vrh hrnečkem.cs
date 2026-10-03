using CardGame;

public class Vrh_hrnečkem : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is DamageableActor target) DealSpellDamage(target, 2, sender);
        GameManager.Instance.AddCardToHandByID(GetOwner(sender), 212);
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
