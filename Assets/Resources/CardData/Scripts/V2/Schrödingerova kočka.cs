using CardGame;

public class Schrödingerova_kočka : CardScriptBase
{
    protected override void OnExperimentBeforeAttack(Minion attacker, Minion.TargetedEventEventArgs e)
    {
        if (attacker.Owner == Experiment.Owner || !GameManager.Instance.IsOpponentTurn(Experiment.Owner)) return;
        ConsumeExperiment();
        if (RandomRange(0, 2) == 0)
        {
            attacker.Death();
            e.target = null;
        }
        else GameManager.Instance.HPCounters[Experiment.Owner].Heal(8);
    }
}
