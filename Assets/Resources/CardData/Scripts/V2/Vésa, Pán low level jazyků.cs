using CardGame;

public class Vésa__Pán_low_level_jazyků : CardScriptBase
{
    const int bugID = 314;
    protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e) =>
        GameManager.Instance.SummonMinion(GetOwner(sender), bugID);
}
