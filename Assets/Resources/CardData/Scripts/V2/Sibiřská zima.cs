using System.Linq;
using CardGame;

public class Sibiřská_zima : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        Minion[] targets = GameManager.Instance.GetAllMinionsOwnedBy(GetOwner(sender).Other()).ToArray();
        foreach (Minion minion in targets) DealSpellDamage(minion, 2, sender);
        foreach (Minion minion in targets)
            if (minion.Health > 0) minion.Frozen = true;
    }
}
