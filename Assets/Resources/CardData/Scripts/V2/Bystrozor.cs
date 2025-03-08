using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System;

public class Bystrozor : CardScriptBase
{
    protected override int SetSpellDamage() => 1;
    protected override void OnDeath(object sender, EventArgs e) {
        DrawCard(true, sender);
    }
}