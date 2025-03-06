using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Drakfyzák : CardScriptBase
{
    
    //TODO spelldamage
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) {
        spelldamage = 1;
        DrawCard(true,sender);
    }
}