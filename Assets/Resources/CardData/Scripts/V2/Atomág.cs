using System.Linq;
using CardGame;

public class Atomág : CardScriptBase
{
    protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e)
    {
        foreach (Minion minion in GameManager.Instance.GetAllMinionsOwnedBy(GetOwner(sender).Other()).ToArray())
            minion.Damage(1);
    }
}
