using CardData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            GetComponent<Image>().sprite = Resources.Load<Sprite>("CardData/" + expansion + "/" + cardData.name);
        }
        return this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
