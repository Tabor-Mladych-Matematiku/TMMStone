using CardGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardListBuilder : MonoBehaviour
{
    [SerializeField] CardDeckbuilderListing CardListingPrefab;
    [SerializeField] DeckBuilderDeck deckBuilderDeck;
    // Start is called before the first frame update
    void Start()
    {
        Dictionary<int,CardData.CardData> cards = CardData.CDJsonUtils.LoadCardDatabase();
        foreach (var item in cards)
        {
            if (item.Value.type == "Token" || item.Value.type == "Spelltoken") continue;
            CardDeckbuilderListing instance = Instantiate(CardListingPrefab, transform);
            instance.name = item.Value.name;
            instance.Instantiate(item.Key,item.Value).GetComponent<Button>().onClick.AddListener(()=> {
                Debug.Log("Pressed: "+item.Value.name);
                deckBuilderDeck.AddCard(item.Key,item.Value);
            });
        }
    }

    
}
