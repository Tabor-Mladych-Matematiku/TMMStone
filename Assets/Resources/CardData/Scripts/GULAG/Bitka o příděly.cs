using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;
using System.Linq;

public class Bitka_o_příděly : CardScriptBase
{
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) {
        List<Minion> minions = GameManager.Instance.AllMinions.ToList();
        Minion spared = GetRandomMinion(sender);
        if (spared == null) return;

        foreach (Minion minion in minions)
        {
            if (minion != null && minion != spared) minion.Death();
        }
    }
}
