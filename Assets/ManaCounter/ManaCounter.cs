using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaCounter : MonoBehaviour
{
    int m = 0;
    public int Mana
    {
        get => m;
        set
        {
            m = value;
            for (int i = 0; i < m; i++)
            {
                SpriteRenderers[i].color = (i>= mm) ? ExtraManaColor: Color.white;
            }
            for (int i = m; i < SpriteRenderers.Length; i++)
            {
                SpriteRenderers[i].color = (i>=mm)?HiddenColor: DimColor;
            }
        }
    }
    int mm=0;
    public int MaxMana
    {
        get => mm;
        set
        {
            mm = value;
            for (int i = 0; i < mm; i++)
            {
                if(SpriteRenderers[i].color==HiddenColor) SpriteRenderers[i].color = DimColor;
            }
            for (int i = mm; i < SpriteRenderers.Length; i++)
            {
                SpriteRenderers[i].color = HiddenColor;
            }
        }
    }
    readonly SpriteRenderer[] SpriteRenderers = new SpriteRenderer[10];
    private Color DimColor = new (0.5f, 0.5f, 0.5f);
    private Color ExtraManaColor = new(0, 1, 0);
    private Color HiddenColor = Color.clear;
    void Start()
    {
        for (int i = 0; i < SpriteRenderers.Length; i++) {
            SpriteRenderers[i] = transform.GetChild(i).GetComponent<SpriteRenderer>();
        }
    }
}
