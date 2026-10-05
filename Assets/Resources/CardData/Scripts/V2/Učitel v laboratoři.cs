using System;
using System.Collections.Generic;
using CardGame;

public class Učitel_v_laboratoři : CardScriptBase
{
    protected override Dictionary<Type, ManaModsModifier> ManaCostMods => new()
    {
        [typeof(Effect)] = source => card =>
            card.Owner == source.GetComponent<Effect>().Owner && card.IsExperiment ? -99999 : 0//TODO: This is a little hacky but this 0 cost should override everything. But test it if it does not blow up
    };

    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) =>
        PlaceEffect(sender, GetOwner(sender));

    protected override void OnBeforeSpellPlayed(Card spell, GameManager.CancelableCardEventArgs e)
    {
        if (spell.IsExperiment && TryGetComponent(out Effect effect) && spell.Owner == effect.Owner)
            effect.Destroy();
    }

    protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e)
    {
        if (sender is Effect effect) effect.Destroy();
    }
}
