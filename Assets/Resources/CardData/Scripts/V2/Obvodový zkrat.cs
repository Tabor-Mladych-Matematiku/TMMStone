using System.Linq;
using CardGame;

public class Obvodový_zkrat : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        foreach (Minion minion in GameManager.Instance.AllMinions.ToArray())
        {
            minion.MaxHealth = 1;
            minion.Health = 1;
        }
    }
}
