using CardGame;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Minion : MonoBehaviour
{
    private int h;
    private int a;
    [SerializeField] TextMesh AttackLabel;
    [SerializeField] TextMesh HealthLabel;
    [SerializeField] SpriteRenderer HighlightRim;
    Color highlightColor;
    Color defaultColor;
    public int Health
    {
        get
        {
            return h;
        }
        private set {
            h = value;
            HealthLabel.text = h.ToString();
        }
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
    }
    public void OnMouseEnter()
    {
        HighlightRim.color = highlightColor;
    }
    public void OnMouseExit()
    {
        HighlightRim.color =defaultColor;
    }
}
