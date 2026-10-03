using System.Linq;
using CardGame;

public class Honzovo_auto : CardScriptBase
{
    protected override void OnTableActorStartOwnTurn(object sender, GameActor.TurnEventArgs e)
    {
        Minion car = (Minion)sender;
        Minion honza = GameManager.Instance.GetAllMinionsOwnedBy(car.Owner.Other())
            .Concat(GameManager.Instance.GetAllMinionsOwnedBy(car.Owner))
            .FirstOrDefault(minion => minion.CardID == 54);
        if (honza != null) car.ForceAttack(honza);
    }
}
