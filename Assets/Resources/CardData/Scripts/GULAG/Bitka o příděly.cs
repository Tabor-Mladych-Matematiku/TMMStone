using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;
using System.Linq;

public class Bitka_o_příděly : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) {
        var minions = GameManager.Instance.AllMinions;
        var en = minions.GetEnumerator();
        Minion spared = GetRandomMinion(sender);
        for (int i = 0; i < minions.Count(); i++)
        {
            en.MoveNext();
            if (en.Current != spared)en.Current.Death();
        }
    }
}