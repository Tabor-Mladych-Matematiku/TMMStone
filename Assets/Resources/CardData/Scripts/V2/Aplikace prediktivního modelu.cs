using CardGame;

public class Aplikace_prediktivního_modelu : CardScriptBase
{
    protected override void OnExperimentBeforeSpellPlayed(Card spell, GameManager.CancelableCardEventArgs e)
    {
        if (spell.Owner == Experiment.Owner || !GameManager.Instance.IsOpponentTurn(Experiment.Owner)) return;
        e.Cancel = true;
        ConsumeExperiment();
    }
}
