using CardGame;

public class Chaloupka_na_kuří_nožce : CardScriptBase
{
    public override bool Taunt => true;

    protected override void OnTableActorStartOwnTurn(object sender, GameActor.TurnEventArgs e)
    {
        var minion = (Minion)sender;
        GameManager.Instance.TakeControl(minion, minion.Owner.Other());
    }
}
