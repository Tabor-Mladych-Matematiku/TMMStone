using CardGame;

public class Klonovací_přístroj : CardScriptBase
{
    protected override void OnExperimentMinionDied(Minion minion)
    {
        if (minion.Owner != Experiment.Owner || !GameManager.Instance.IsOpponentTurn(Experiment.Owner)) return;
        int cardID = minion.CardID;
        GameManager.P owner = Experiment.Owner;
        ConsumeExperiment();
        GameManager.Instance.SummonMinion(owner, cardID);
    }
}
