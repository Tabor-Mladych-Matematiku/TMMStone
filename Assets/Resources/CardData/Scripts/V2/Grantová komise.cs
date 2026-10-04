using System.Linq;
using CardGame;

public class Grantová_komise : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        var owner = GetOwner(sender);
        if (GameManager.Instance.GetAllEffectsOwnedBy(owner).Any(effect => effect.isExperiment))
            GameManager.Instance.ManaCounters[owner].Mana += 2;
    }
}
