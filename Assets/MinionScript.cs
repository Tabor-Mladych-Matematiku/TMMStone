using CardGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionScript : MonoBehaviour {
    Image image;
    Color highlightc = new(0, 1, 0, 0.3f);

    void Start() {
        image = GetComponent<Image>();
    }
    private void OnMouseOver()
    {
        Card holding = GameManager.Instance.cursor;
        if (holding!=null && holding.cardType==Card.CardType.Minion && transform.childCount == 0) { 
            image.color = highlightc;
            GameManager.Instance.highlightedSlot = transform;
        }
    }
    private void OnMouseExit()
    {
        image.color = new(0,0,0,0);
        GameManager.Instance.highlightedSlot = null;
    }
}
