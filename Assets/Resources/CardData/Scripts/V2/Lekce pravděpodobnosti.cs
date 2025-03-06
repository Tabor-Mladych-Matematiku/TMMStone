using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;
using System.Linq;

public class Lekce_pravděpodobnosti : CardScriptBase
{
    const int damage = 4;
    protected override void OnSelfPlayed(object sender, TargetlessEventArgs e) {
        ToRandomEnemyCharacterDo(sender, (m) => DealSpellDamage(m,damage,sender));
    }
}