using CardData;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using CardGame;
using UnityEngine.SceneManagement;

public class DeckBuilderDeck : MonoBehaviour
{
    private const string DefaultDeckName = "Unnamed deck";
    [SerializeField]Button deckButton;
    [SerializeField] CardInDeckUI listing;
    [SerializeField] TMP_InputField deckNameInput;

    readonly List<int> cards = new();
    readonly List<CardData.CardData> cardDataInDeck = new();
    readonly List<CardInDeckUI> cardButtons = new();
    string deckName = DefaultDeckName;
    public event Action<string> ClassChanged;

    public string ActiveClass { get; private set; }

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
        CreateBackButton();
    }

    public void AddCard(int cardID,CardData.CardData cardData)
    {
        if (!GameManager.DEBUG && !string.IsNullOrEmpty(ActiveClass) &&
            !string.Equals(cardData.Class, "Neutral", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(cardData.Class, ActiveClass, StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"Cannot add {cardData.name}: this is a {ActiveClass} deck.");
            return;
        }

        if (cards.Count >= 30)
        {
            Debug.Log("Too many cards");
            return;
        }
        CardInDeckUI instance = Instantiate(listing, transform);
        instance.GetComponentInChildren<TextMeshProUGUI>().text = cardData.name;
        string expansion = CDJsonUtils.expansionMapping[cardData.expansion];
        Image image = instance.GetComponent<Image>();
        CardArt.Load(CardArt.PlainAddress(expansion, cardData.name), sprite =>
        {
            if (instance != null && sprite != null) image.sprite = sprite;
        });
        instance.GetComponent<Button>().onClick.AddListener(() => {
            RemoveCard(cardID);
            Destroy(instance.gameObject);
        });
        cardButtons.Add(instance);
        cards.Add(cardID);
        cardDataInDeck.Add(cardData);
        UpdateActiveClass();
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
        cardDataInDeck.RemoveAt(index);
        UpdateActiveClass();
        if (cards.Count < 30)
        {
            deckButton.enabled = false;
        }
        deckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Deck (" + cards.Count + "/30)";
    }
    public void SaveDeck()
    {
        string savedName = DeckStorage.Save(deckName, MiniJson.JsonEncode(cards));
        Debug.Log("Saved deck: " + savedName);

        foreach (var item in cardButtons)
        {
            Destroy(item.gameObject);
        }
        cards.Clear();
        cardDataInDeck.Clear();
        cardButtons.Clear();
        UpdateActiveClass();
        deckButton.enabled = false;
        deckName = DefaultDeckName;
        deckNameInput.text = string.Empty;
        deckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Deck (" + cards.Count + "/30)";
    }

    private void UpdateActiveClass()
    {
        string nextClass = null;
        foreach (CardData.CardData data in cardDataInDeck)
        {
            if (!string.IsNullOrWhiteSpace(data.Class) &&
                !string.Equals(data.Class, "Neutral", StringComparison.OrdinalIgnoreCase))
            {
                nextClass = data.Class;
                break;
            }
        }

        if (string.Equals(ActiveClass, nextClass, StringComparison.OrdinalIgnoreCase)) return;
        ActiveClass = nextClass;
        ClassChanged?.Invoke(ActiveClass);
    }

    private void CreateBackButton()
    {
        Button backButton = Instantiate(deckButton, deckButton.transform.parent);
        backButton.name = "BackButton";
        backButton.enabled = true;
        backButton.interactable = true;
        backButton.onClick = new Button.ButtonClickedEvent();
        backButton.onClick.AddListener(() => SceneManager.LoadScene("MenuScene"));
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";

        RectTransform rect = backButton.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(140, 55);
    }
}
