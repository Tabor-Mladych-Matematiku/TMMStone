using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Lehce_zmutovaný_pejsek : CardScriptBase
{
    const int UranID = 239;
    protected override void OnAfterAttack(object sender, Minion.TargetedEventEventArgs e) {

        if (e.target is Minion minion && !minion.Alive()) {
            GameManager.Instance.AddCardToHandByID(GetOwner(sender), UranID);
        }
    }

    protected override void OnBattleCry(object sender, Minion.TargetedEventEventArgs e) {
        ((Minion)sender).Charge();
    }
}