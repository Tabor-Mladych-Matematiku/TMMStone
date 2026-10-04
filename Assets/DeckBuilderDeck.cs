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
            RefreshSaveButton();
        });
        RefreshSaveButton();
        CreateBackButton();
    }

    public void AddCard(int cardID,CardData.CardData cardData)
    {
        if (!GameManager.DEBUG && !CanAddCard(cardID, cardData, out string validationError))
        {
            Debug.LogWarning(validationError);
            return;
        }

        if (GameManager.DEBUG && cards.Count >= 30)
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
        RefreshSaveButton();
    }
    public void RemoveCard(int cardID) {
        int index = cards.IndexOf(cardID);//Prasečina
        cardButtons.RemoveAt(index);
        cards.RemoveAt(index);
        cardDataInDeck.RemoveAt(index);
        UpdateActiveClass();
        RefreshSaveButton();
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
        RefreshSaveButton();
    }

    private bool CanAddCard(int cardID, CardData.CardData cardData, out string error)
    {
        bool isAbility = IsAbility(cardData);
        int copies = 0;
        foreach (int existingID in cards)
            if (existingID == cardID) copies++;

        if (isAbility && HasAbility())
        {
            error = "A deck can contain only one Schopnost card.";
            return false;
        }

        int copyLimit = string.Equals(cardData.rarity, "Org", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
        if (copies >= copyLimit)
        {
            error = copyLimit == 1
                ? $"A deck can contain only one copy of {cardData.name}."
                : $"A deck can contain only two copies of {cardData.name}.";
            return false;
        }

        if (!isAbility && DeckSize >= 30)
        {
            error = "The deck already contains 30 cards. Schopnost does not count toward this limit.";
            return false;
        }

        if (!string.IsNullOrEmpty(ActiveClass) &&
            !string.Equals(cardData.Class, "Neutral", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(cardData.Class, ActiveClass, StringComparison.OrdinalIgnoreCase))
        {
            error = $"Cannot add {cardData.name}: this is a {ActiveClass} deck.";
            return false;
        }

        error = null;
        return true;
    }

    private int DeckSize
    {
        get
        {
            int count = 0;
            foreach (CardData.CardData data in cardDataInDeck)
                if (!IsAbility(data)) count++;
            return count;
        }
    }

    private bool HasAbility()
    {
        foreach (CardData.CardData data in cardDataInDeck)
            if (IsAbility(data)) return true;
        return false;
    }

    private static bool IsAbility(CardData.CardData data) =>
        string.Equals(data.type, "Schopnost", StringComparison.OrdinalIgnoreCase);

    private void RefreshSaveButton()
    {
        int countedCards = GameManager.DEBUG ? cards.Count : DeckSize;
        deckButton.enabled = countedCards == 30;
        string abilitySuffix = !GameManager.DEBUG && HasAbility() ? " + Schopnost" : string.Empty;
        deckButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Save Deck ({countedCards}/30{abilitySuffix})";
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
