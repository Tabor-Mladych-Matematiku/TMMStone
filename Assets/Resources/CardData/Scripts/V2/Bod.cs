using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Bod : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e){
        GameManager.Instance.ManaCounters[GetOwner(sender)].Mana += 1;
    }
}