using System;
using System.Linq;
using CardGame;

public class Férovkový_obr : CardScriptBase
{
    protected override Func<Card, int> ManaCostModifier =>
        card => -GameManager.Instance.AllMinions.Count();
}
