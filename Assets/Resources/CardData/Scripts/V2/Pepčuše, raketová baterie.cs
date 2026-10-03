using System.Linq;
using CardGame;

public class Pepčuše__raketová_baterie : CardScriptBase
{
    protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e)
    {
        DamageableActor[] targets = GameManager.Instance.AllCharacters
            .Where(character => character != (DamageableActor)sender).ToArray();
        foreach (DamageableActor target in targets) target.Damage(2);
    }
}
