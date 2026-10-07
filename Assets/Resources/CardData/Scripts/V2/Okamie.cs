using System.Linq;
using CardGame;

public class Okamie : CardScriptBase
{
    protected override void OnSelfSummoned(object sender, Minion.SummonedEventArgs e) => UpdateBonus();
    protected override void OnBoardChanged() => UpdateBonus();

    private void UpdateBonus()
    {
        if (!TryGetComponent(out Minion self) || !self.Alive() || self.GetComponentInParent<CardSlot>() == null) return;
        DynamicAttackModifier = GameManager.maxMinionSlots - GameManager.Instance.GetAllMinionsOwnedBy(self.Owner).Count();
    }
}
