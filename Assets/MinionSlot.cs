using CardGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionSlot : CardSlot
{
    Image image;
    Color highlightc = new(0, 1, 0, 0.3f);

    void Start() {
        image = GetComponent<Image>();
    }
    private void OnMouseOver()
    {
        if (Owner != GameManager.P.P1) return;
        Card holding = GameManager.Instance.cursor;
        if (holding!=null && holding.cardType==Card.CardType.Minion && transform.childCount == 0) { 
            image.color = highlightc;
            GameManager.Instance.highlightedSlot = transform;
        }
    }
    private void OnMouseExit()
    {
        if (Owner!=GameManager.P.P1) return;
        image.color = new(0,0,0,0);
        GameManager.Instance.highlightedSlot = null;
    }
    public void RemoveMinion()
    {
        Destroy(transform.GetComponent<Minion>());
        GameManager.Instance.AddToGrave(PopCard(),Owner);
    }
}
