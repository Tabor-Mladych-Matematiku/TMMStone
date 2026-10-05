using CardGame;

public class Útěk_experimentů : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is not DamageableActor target) return;
        int earlierCards = System.Math.Max(0, GameManager.Instance.Stats.GetCardsPlayedThisTurn(GetOwner(sender)) - 1);
        DealSpellDamage(target, earlierCards > 0 ? 4 : 2, sender);
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
