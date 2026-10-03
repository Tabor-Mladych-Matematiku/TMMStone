using System.Linq;
using CardGame;

public class Zavedení_o_n__notace : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        Minion[] targets = GameManager.Instance.AllMinions.Where(minion => minion.Attack <= 2).ToArray();
        foreach (Minion minion in targets)
            minion.Death();
    }
}
