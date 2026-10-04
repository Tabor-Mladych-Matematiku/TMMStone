using CardGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CardListBuilder : MonoBehaviour
{
    [SerializeField] CardDeckbuilderListing CardListingPrefab;
    [SerializeField] DeckBuilderDeck deckBuilderDeck;
    readonly List<CardDeckbuilderListing> listings = new();

    void Awake()
    {
        deckBuilderDeck.ClassChanged += ApplyClassFilter;
    }

    void OnDestroy()
    {
        if (deckBuilderDeck != null)
            deckBuilderDeck.ClassChanged -= ApplyClassFilter;
    }
    // Start is called before the first frame update
    void Start()
    {
        Dictionary<int,CardData.CardData> cards = CardData.CDJsonUtils.LoadCardDatabase();
        foreach (var item in cards)
        {
            if (item.Value.type.EndsWith("token", StringComparison.OrdinalIgnoreCase)) continue;
            CardDeckbuilderListing instance = Instantiate(CardListingPrefab, transform);
            listings.Add(instance);
            instance.name = item.Value.name;
            instance.Instantiate(item.Key,item.Value).GetComponent<Button>().onClick.AddListener(()=> {
                Debug.Log("Pressed: "+item.Value.name);
                deckBuilderDeck.AddCard(item.Key,item.Value);
            });
        }
        ApplyClassFilter(deckBuilderDeck.ActiveClass);
    }

    void ApplyClassFilter(string activeClass)
    {
        if (GameManager.DEBUG)
        {
            foreach (CardDeckbuilderListing listing in listings)
                listing.gameObject.SetActive(true);
            return;
        }

        foreach (CardDeckbuilderListing listing in listings)
        {
            bool visible = string.IsNullOrEmpty(activeClass) ||
                string.Equals(listing.CardClass, "Neutral", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(listing.CardClass, activeClass, StringComparison.OrdinalIgnoreCase);
            listing.gameObject.SetActive(visible);
        }
    }
}
