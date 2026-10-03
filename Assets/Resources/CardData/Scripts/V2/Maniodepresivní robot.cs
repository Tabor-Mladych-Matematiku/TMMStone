using System.Linq;
using CardGame;

public class Maniodepresivní_robot : CardScriptBase
{
    protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e)
    {
        if (sender is not Minion minion) return;
        if (GameManager.Instance.GetAllMinionsOwnedBy(minion.Owner).Count() == 1) minion.Death();
    }
}
