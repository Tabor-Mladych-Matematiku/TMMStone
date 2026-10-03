using System.Linq;
using CardGame;

public class Zlý_profesor : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        foreach (Minion minion in GameManager.Instance.GetAllMinionsOwnedBy(GetOwner(sender).Other()).ToArray())
            minion.Attack = 1;
    }
}
