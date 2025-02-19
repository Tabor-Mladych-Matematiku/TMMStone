using CardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Minion : MonoBehaviour
{
    private int h;
    private int a;
    [SerializeField] TextMesh AttackLabel;
    [SerializeField] TextMesh HealthLabel;
    [SerializeField] SpriteRenderer HighlightRim;
    [SerializeField] SpriteRenderer graphic;
    Color highlightColor;
    Color defaultColor;

    public event EventHandler<BattlecryEventArgs> OnBattleCry;
    public event EventHandler OnDeath;
    public class BattlecryEventArgs
    {
        public int target;
    }
    public int Health
    {
        get
        {
            return h;
        }
        private set {
            h = value;
            HealthLabel.text = h.ToString();
            if (value <= 0) Death();
        }
    }

    private void Death()
    {
        OnDeath?.Invoke(this,new());
        transform.parent.GetComponent<MinionSlot>().RemoveMinion();
    }

    public int Attack
    {
        get
        {
            return a;
        }
        private set
        {
            a = math.max(0,value);
            AttackLabel.text = a.ToString();
        }
    }
    private void Awake()
    {
        defaultColor = HighlightRim.color;
        highlightColor =new(defaultColor.r, defaultColor.g, defaultColor.b, 0.8f);
    }
    public void Initialize(Card c)
    {
        Attack = c.stats[0];
        Health = c.stats[1];
        Sprite sprite = Resources.Load<Sprite>("CardPlainImages/" + c.expansion + "/" + c.cardname);
        if (sprite != null) graphic.sprite = sprite;
    }
    public void OnMouseEnter()
    {
        HighlightRim.color = highlightColor;
    }
    public void OnMouseExit()
    {
        HighlightRim.color =defaultColor;
    }

    internal void Battlecry(int target)
    {
        OnBattleCry?.Invoke(this,new() { target = target });
    }
}
