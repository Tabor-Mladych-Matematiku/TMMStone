using CardGame;

public class Fontána_života : CardScriptBase
{
    protected override void OnTableActorEndTurn(object sender, GameActor.TurnEventArgs e) =>
        GameManager.Instance.HPCounters[e.Player].Heal(4);
}
