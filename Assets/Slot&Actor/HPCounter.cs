using CardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HPCounter : CardSlot
{
    [SerializeField] Image[] circles;
    [SerializeField] TextMeshProUGUI HPLabel;
    public int maxHP;
    public Face Face { get => GetComponentInChildren<Face>(); }
    int h;
    public int Health
    {
        get => h;
        set
        {
            h = value;
            HPLabel.text = value.ToString();
            circles[0].gameObject.SetActive(h >= 10);
            circles[1].gameObject.SetActive(h >= 20);
            circles[2].gameObject.SetActive(h >= 30);
            if (h <= 0) Death?.Invoke(this,new()) ;
        }
    }
    public event EventHandler Death;
    public event EventHandler Healed;
    public void Heal(int ammount)
    {
        int newHealth = math.min(Health + ammount, maxHP);
        if(newHealth>Health) Healed?.Invoke(this, new());
        Health = newHealth;
    }
}
