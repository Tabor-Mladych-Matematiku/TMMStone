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
                SpriteRenderers[i].color = Color.white;
            }
            for (int i = m; i < SpriteRenderers.Length; i++)
            {
                SpriteRenderers[i].color = DimColor;
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
                SpriteRenderers[i].gameObject.SetActive(true) ;
            }
            for (int i = mm; i < SpriteRenderers.Length; i++)
            {
                SpriteRenderers[i].gameObject.SetActive(false);
            }
        }
    }
    readonly SpriteRenderer[] SpriteRenderers = new SpriteRenderer[10];
    private Color DimColor;
    void Start()
    {
        for (int i = 0; i < SpriteRenderers.Length; i++) {
            SpriteRenderers[i] = transform.GetChild(i).GetComponent<SpriteRenderer>();
        }
        DimColor = SpriteRenderers[0].color;
    }
}
