using System.Linq;
using CardGame;

public class Prohození_proměnných : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        var damage = GameManager.Instance.AllMinions
            .Select(minion => new { Minion = minion, Amount = minion.Attack }).ToArray();
        foreach (var hit in damage) hit.Minion.Damage(hit.Amount);
    }
}
