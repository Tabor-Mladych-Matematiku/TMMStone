using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Drakfyzák : CardScriptBase
{

    protected override int SetSpellDamage() => 1;
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) {
        DrawCard(true,sender);
    }
}