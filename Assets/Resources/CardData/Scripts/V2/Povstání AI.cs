using System.Linq;
using CardGame;

public class Povstání_AI : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        foreach (Minion minion in GameManager.Instance.GetAllMinionsOwnedBy(GetOwner(sender)).ToArray())
            minion.Buff(1, 1);
    }
}
