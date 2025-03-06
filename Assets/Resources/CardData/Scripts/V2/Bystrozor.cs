using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Bystrozor : CardScriptBase
{
    //TODO Spelldamage
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e)
    {
        spelldamage = 1;
    }
    protected override void OnDeath(object sender, EventArgs e) {
        DrawCard(true, sender);
    }
}