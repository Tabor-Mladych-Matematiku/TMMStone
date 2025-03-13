using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaCounter : MonoBehaviour
{
    int m = 0;
    public int Mana
    {
        get => m;
        set
        {
            m = value;

            for (int i = 0; i < Mathf.Min(m,10); i++)
            {
                ManaCrystalImage[i].color = (i>= mm) ? ExtraManaColor: Color.white;
            }
            for (int i = Mathf.Min(m, 10); i < ManaCrystalImage.Length; i++)
            {
                ManaCrystalImage[i].color = (i>=mm)?HiddenColor: DimColor;
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
                if(ManaCrystalImage[i].color==HiddenColor) ManaCrystalImage[i].color = DimColor;
            }
            for (int i = mm; i < ManaCrystalImage.Length; i++)
            {
                ManaCrystalImage[i].color = HiddenColor;
            }
        }
    }
    readonly Image[] ManaCrystalImage = new Image[10];
    private Color DimColor = new (0.5f, 0.5f, 0.5f);
    private Color ExtraManaColor = new(0, 1, 0);
    private Color HiddenColor = Color.clear;
    void Start()
    {
        for (int i = 0; i < ManaCrystalImage.Length; i++) {
            ManaCrystalImage[i] = transform.GetChild(i).GetComponent<Image>();
        }
    }
}
