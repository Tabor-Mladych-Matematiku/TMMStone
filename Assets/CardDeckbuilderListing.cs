using CardData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CardGame;

[RequireComponent(typeof(Image))]
public class CardDeckbuilderListing : MonoBehaviour
{
    int cardID;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public CardDeckbuilderListing Instantiate(int id, CardData.CardData cardData)
    {
        cardID = id;
        if (CDJsonUtils.expansionMapping.ContainsKey(cardData.expansion))
        {
            string expansion = CDJsonUtils.expansionMapping[cardData.expansion];
            Image image = GetComponent<Image>();
            CardArt.Load(CardArt.FaceAddress(expansion, cardData.name), sprite =>
            {
                if (this != null && sprite != null) image.sprite = sprite;
            });
        }
        return this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
