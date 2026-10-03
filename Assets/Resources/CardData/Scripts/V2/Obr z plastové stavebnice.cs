using System;
using CardGame;

public class Obr_z_plastové_stavebnice : CardScriptBase
{
    protected override Func<Card, int> ManaCostModifier => card =>
        -(GameManager.Instance.HPCounters[card.Owner].maxHP
          - GameManager.Instance.HPCounters[card.Owner].Health);
}
