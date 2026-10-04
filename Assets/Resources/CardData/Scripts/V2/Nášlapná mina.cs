using System;
using CardGame;

public class Nášlapná_mina : CardScriptBase
{
    protected override void OnExperimentMinionPlayed(Minion minion)
    {
        if (minion.Owner == Experiment.Owner || !GameManager.Instance.IsOpponentTurn(Experiment.Owner)) return;
        int damage = CalculateSpellDamage(6, Experiment);
        int overflow = Math.Max(0, damage - minion.Health);
        ConsumeExperiment();
        if (minion.Immune) return;//This is here because overflow could be greater than 0 even when minion is immune
        minion.Damage(damage);
        if (overflow > 0) GameManager.Instance.HPCounters[minion.Owner].Face.Damage(overflow);
    }
}
