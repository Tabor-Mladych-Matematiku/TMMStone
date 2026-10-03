using System.Linq;
using CardGame;

public class Duch_dělníkův : CardScriptBase
{
    public override bool Taunt => true;

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        GameManager.P owner = GetOwner(sender);
        Minion self = (Minion)sender;
        bool hasOtherGhost = GameManager.Instance.GetAllMinionsOwnedBy(owner)
            .Any(minion => minion != self && minion.HasTag("Duch"));
        GameManager.Instance.SummonMinion(owner, 208);
        if (hasOtherGhost) GameManager.Instance.SummonMinion(owner, 208);
    }
}
