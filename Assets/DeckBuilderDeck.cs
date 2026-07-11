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
    [SerializeField] TMP_InputField deckNameInput;

    List<int> cards = new();
    List<CardInDeckUI> cardButtons = new();
    string deckName = null;
    private void Start()
    {
        deckButton.enabled = false;
        deckNameInput.onEndEdit.AddListener((string value) => {
            Debug.Log("Pervol black magic");
            deckName = CDJsonUtils.SanitizeToClassName(value);//Todo: Maybe do this better
            if (string.IsNullOrEmpty(deckName))
            {
                deckButton.enabled = false;
            }
            else
            {
                if (cards.Count == 30)
                {
                    Debug.Log("Pervol black magic elektrické boogaloo");
                    deckButton.enabled = true;
                }
            }
        });
    }

    public void AddCard(int cardID,CardData.CardData cardData)
    {
        if (cards.Count >= 30)
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
        if (cards.Count == 30 && deckName!=null)
        {
            deckButton.enabled = true;
        }
        deckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Deck (" + cards.Count + "/30)";
    }
    public void RemoveCard(int cardID) {
        int index = cards.IndexOf(cardID);//Praseèina
        cardButtons.RemoveAt(index);
        cards.RemoveAt(index);
        if (cards.Count < 30)
        {
            deckButton.enabled = false;
        }
        deckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Deck (" + cards.Count + "/30)";
    }
    public void SaveDeck()
    {
        string saveFolder = Path.Combine(Application.persistentDataPath, "Decks");

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
        string savePath = Path.Combine(saveFolder, deckName+".json");
        Debug.Log("Saving deck to: " + savePath);
        File.WriteAllText(savePath, MiniJson.JsonEncode(cards));

        foreach (var item in cardButtons)
        {
            Destroy(item.gameObject);
        }
        cards.Clear();
        cardButtons.Clear();
        deckButton.enabled = false;
        deckName = null;
        deckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Deck (" + cards.Count + "/30)";
    }


}
