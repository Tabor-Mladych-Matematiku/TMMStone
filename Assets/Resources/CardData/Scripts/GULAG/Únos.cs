using CardGame;

public class Únos : CardScriptBase
{
    protected override void OnExperimentMinionPlayed(Minion minion)
    {
        if (minion.Owner == Experiment.Owner || !GameManager.Instance.IsOpponentTurn(Experiment.Owner)) return;
        GameManager.P owner = Experiment.Owner;
        ConsumeExperiment();
        if (GameManager.Instance.GetNextFreeMinionSlot(owner) == -1 || !minion.Alive()) return;

        int captiveID = minion.CardID;
        minion.RemoveWithoutDeath();
        if(GameManager.Instance.SummonMinion(owner, 227).TryGetComponent(out Pytel bag) && bag!=null)
            bag.SetCaptive(captiveID);
    }
}
