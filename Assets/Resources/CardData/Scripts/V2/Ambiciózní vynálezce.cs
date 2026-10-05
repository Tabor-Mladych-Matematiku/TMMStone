using System.Linq;
using CardData;
using CardGame;

public class Ambiciózní_vynálezce : CardScriptBase
{
    protected override void OnSelfSummoned(object sender, Minion.TargetedEventEventArgs e) => UpdateBonus();
    protected override void OnBoardChanged() => UpdateBonus();

    private void UpdateBonus()
    {
        if (!TryGetComponent(out Minion self) || !self.Alive() || self.GetComponentInParent<CardSlot>() == null) return;
        bool shouldApply = GameManager.Instance.GetAllMinionsOwnedBy(self.Owner)
            .Any(minion => minion != self && minion.HasTag(CardTag.Stroj));
        DynamicAttackModifier = shouldApply ? 2 : 0;
    }
}
