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
        image.color = highlightc;
    }
    private void OnMouseExit()
    {
        image.color = new(0,0,0,0);
    }
}
