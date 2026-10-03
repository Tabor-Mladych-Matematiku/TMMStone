using System.Linq;
using CardGame;

public class Anihilace_pozitronů : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        foreach (Minion minion in GameManager.Instance.AllMinions.ToArray()) minion.Death();
    }
}
