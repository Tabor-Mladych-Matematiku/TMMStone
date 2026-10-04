using System.Linq;
using CardGame;

public class Doma_namíchaná_trhavina : TargetableCardScriptBase
{
    protected override bool TargetValidate(TableActor target) =>
        target is Minion && target.Owner != GetComponent<Card>().Owner;

    protected override void OnSelfPlayed(object sender, Card.CardPlayedEventArgs e)
    {
        var target = (Minion)e.Target;
        var neighbors = GameManager.Instance.GetAdjacentMinions(target).ToArray();
        DealSpellDamage(target,5,sender);
        foreach (var neighbor in neighbors)
            if (neighbor != null && neighbor.Alive()) DealSpellDamage(neighbor,2,sender);
    }
}
