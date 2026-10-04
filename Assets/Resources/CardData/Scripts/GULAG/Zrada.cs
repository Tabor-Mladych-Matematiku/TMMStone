using System.Linq;
using CardGame;

public class Zrada : CardScriptBase
{
    protected override void OnExperimentBeforeAttack(Minion attacker, Minion.TargetedEventEventArgs e)
    {
        if (attacker.Owner == Experiment.Owner || !GameManager.Instance.IsOpponentTurn(Experiment.Owner)) return;
        if (e.target is not Face face || face.Owner != Experiment.Owner) return;
        Minion[] neighbours = GameManager.Instance.GetAdjacentMinions(attacker).Where(minion => minion.Alive()).ToArray();
        if (neighbours.Length == 0) return;
        e.target = neighbours[RandomRange(0, neighbours.Length)];
        ConsumeExperiment();
    }
}
