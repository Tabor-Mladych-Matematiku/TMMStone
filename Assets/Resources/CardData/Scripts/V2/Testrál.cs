using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Testrál : CardScriptBase
{
    protected override void OnDeath(object sender, EventArgs e) { 
        DrawCard(true, sender);
    }
}