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
            if (en.Current.gameObject == null) continue;//Bandaid solution for the belowmentioned bug
            if (en.Current != spared)en.Current.Death();//TODO: This will cause bugs since OnDeath effects may kill the minions that will come after this one.
            //Probably the OnDeaths should be queued up and not forcehappening but thats for later.
        }
    }
}