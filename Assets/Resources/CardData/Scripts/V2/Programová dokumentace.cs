using CardGame;

public class Programová_dokumentace : CardScriptBase
{
    protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e) =>
        DrawCard(true, sender);
}
