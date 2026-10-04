using CardData;
using CardGame;

public class Lenička__Matka_přírody : CardScriptBase
{
    protected override void OnMinionSummoned(Minion minion)
    {
        if (TryGetComponent(out Minion self) && self.Alive() && minion != self
            && minion.Owner == self.Owner && minion.HasTag(CardTag.Zvíře))
            minion.Charge();
    }
}
