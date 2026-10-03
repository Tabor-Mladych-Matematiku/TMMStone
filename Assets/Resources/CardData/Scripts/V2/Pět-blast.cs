using CardGame;

public class Pět_blast : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        const int bugID = 314;
        if (e.Target is DamageableActor target) DealSpellDamage(target, 3, sender);
        GameManager.Instance.SummonMinion(GetOwner(sender), bugID);
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
