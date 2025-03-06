using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Jana_Z___Zdravotnice : CardScriptBase
{
    const int healammount = 8;
    protected override void OnTableActorEndOwnTurn(object sender, GameActor.TurnEventArgs e)
    {
        ToRandomFriendlyCharacterDo(sender, (m) => m.Heal(healammount));
    }
}