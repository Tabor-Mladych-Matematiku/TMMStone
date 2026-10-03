using System.Linq;
using CardGame;

[UnityEngine.RequireComponent(typeof(Card))]
public class DDoS_útok : TargetableCardScriptBase
{
    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        if (e.Target is not DamageableActor primary) return;
        DamageableActor[] others = GameManager.Instance
            .GetAllCharactersOwnedBy(GetOwner(sender).Other())
            .Where(character => character != primary).ToArray();
        DealSpellDamage(primary, 4, sender);
        foreach (DamageableActor target in others) DealSpellDamage(target, 1, sender);
    }

    protected override bool TargetValidate(TableActor target) =>
        target is DamageableActor && target.Owner == GetComponent<Card>().Owner.Other();
}
