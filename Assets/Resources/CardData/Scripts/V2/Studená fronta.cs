using CardGame;
using System.Linq;

public class Studená_fronta : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        foreach (Minion minion in GameManager.Instance
            .GetAllMinionsOwnedBy(GetOwner(sender).Other()).ToArray())
            DealSpellDamage(minion, 1, sender);
    }
}
