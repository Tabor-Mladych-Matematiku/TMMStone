using System.Linq;
using CardGame;

public class Aritmetická_destrukce : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is not DamageableActor target) return;
        int[] requiredHealth = { 1, 2, 3, 4, 5 };
        var healths = GameManager.Instance.GetAllMinionsOwnedBy(GetOwner(sender))
            .Select(minion => minion.Health).ToArray();
        if (requiredHealth.All(healths.Contains)) DealSpellDamage(target, 15, sender);
    }

    protected override bool TargetValidate(TableActor target) => target is DamageableActor;
}
