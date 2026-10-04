using System;
using System.Collections.Generic;
using CardGame;
using CardData;

public class Lví_královna : CardScriptBase
{
    protected override Dictionary<Type, ManaModsModifier> ManaCostMods => new()
    {
        [typeof(Minion)] = source => card =>
            card.Owner == source.GetComponent<TableActor>().Owner && card.HasTag(CardTag.Zvíře) ? -1 : 0
    };
}
