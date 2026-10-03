using System.Linq;
using CardGame;

public class Bouřňák : CardScriptBase
{
    public override bool Taunt => true;

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        // OnSelfPlayed runs before this card's minion is created on the board.
        foreach (Minion minion in GameManager.Instance.AllMinions.ToArray())
            minion.Damage(2);
    }
}
