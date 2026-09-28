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
    private const string DefaultDeckName = "Unnamed deck";
    [SerializeField]Button deckButton;
    [SerializeField] CardInDeckUI listing;
    [SerializeField] TMP_InputField deckNameInput;

    readonly List<int> cards = new();
    readonly List<CardInDeckUI> cardButtons = new();
    string deckName = DefaultDeckName;
    private void Start()
    {
        deckButton.enabled = false;
        deckNameInput.onEndEdit.AddListener((string value) => {
            Debug.Log("Pervol black magic");
            deckName = string.IsNullOrWhiteSpace(value)
                ? DefaultDeckName
                : CDJsonUtils.SanitizeToClassName(value);
            if (cards.Count == 30)
            {
                Debug.Log("Pervol black magic elektrické boogaloo");
                deckButton.enabled = true;
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
        int index = cards.IndexOf(cardID);//Prasečina
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
        string savePath = GetUniqueSavePath(saveFolder, deckName);
        Debug.Log("Saving deck to: " + savePath);
        File.WriteAllText(savePath, MiniJson.JsonEncode(cards));

        foreach (var item in cardButtons)
        {
            Destroy(item.gameObject);
        }
        cards.Clear();
        cardButtons.Clear();
        deckButton.enabled = false;
        deckName = DefaultDeckName;
        deckNameInput.text = string.Empty;
        deckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Deck (" + cards.Count + "/30)";
    }

    private static string GetUniqueSavePath(string saveFolder, string requestedName)
    {
        string savePath = Path.Combine(saveFolder, requestedName + ".json");
        int suffix = 1;

        while (File.Exists(savePath))
        {
            savePath = Path.Combine(saveFolder, $"{requestedName} ({suffix}).json");
            suffix++;
        }

        return savePath;
    }
}
