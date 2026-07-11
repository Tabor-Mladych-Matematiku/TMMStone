using CardData;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class DeckBuilderDeck : MonoBehaviour
{
    [SerializeField]Button deckButton;
    [SerializeField] CardInDeckUI listing;

    List<int> cards = new();
    List<CardInDeckUI> cardButtons = new();
    private void Start()
    {
        deckButton.enabled = false;
    }

    public void AddCard(int cardID,CardData.CardData cardData)
    {
        if (cards.Count > 30)
        {
            Debug.Log("Too many cards");
            return;
        }
        CardInDeckUI instance = Instantiate(listing, transform);
        instance.GetComponentInChildren<TextMeshProUGUI>().text = cardData.name;
        string expansion = CDJsonUtils.expansionMapping[cardData.expansion];
        instance.GetComponent<Image>().sprite = Resources.Load<Sprite>("CardPlainImages/" + expansion + "/" + cardData.name);
        instance.GetComponent<Button>().onClick.AddListener(() => {
            RemoveCard(cardID);
            Destroy(instance.gameObject);
        });
        cardButtons.Add(instance);
        cards.Add(cardID);
        if (cards.Count == 30)
        {
            deckButton.enabled = true;
        }
    }
    public void RemoveCard(int cardID) {
        int index = cards.IndexOf(cardID);//Praseèina
        cardButtons.RemoveAt(index);
        cards.RemoveAt(index);
        if (cards.Count < 30)
        {
            deckButton.enabled = false;
        }
    }
    public void SaveDeck()
    {
        string saveFolder = Path.Combine(Application.persistentDataPath, "Decks");

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        string savePath = Path.Combine(saveFolder, "deck.json");
        Debug.Log("Saving deck to: " + savePath);
        File.WriteAllText(savePath, MiniJson.JsonEncode(cards));

        foreach (var item in cardButtons)
        {
            Destroy(item.gameObject);
        }
        cards.Clear();
        deckButton.enabled = false;
    }


}
