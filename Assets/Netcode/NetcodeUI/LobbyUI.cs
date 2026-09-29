using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] Button CreateLobbyButton;
    [SerializeField] Toggle PrivacyToggle;
    [SerializeField] GameObject lobbyListingPrefab;
    [SerializeField] Transform container;
    [SerializeField] Button refreshButton;
    [SerializeField] TMP_InputField CreateLobbyName;
    [SerializeField] TMP_InputField PlayerNameField;
    [SerializeField] TMP_InputField JoinCodeField;
    [SerializeField] Button JoinByCodeButton;
    [SerializeField] Button QuickJoinButton;
    [SerializeField] TextMeshProUGUI ValidationMessage;
    public string PlayerName
    {
        get {
            string r = PlayerNameField.text.Trim();
#if UNITY_ANDROID
        if (r == "") r = "Androidymous";
#else
        if (r == "") r = "An only mouse";
#endif
        if(Debug.isDebugBuild) r += UnityEngine.Random.Range(0, 200);
            return Sanitize(r);
        }
    }
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        EnsureJoinControls();

        CreateLobbyButton.onClick.AddListener(() =>
        {
            if (!TryValidateLobbyName(CreateLobbyName.text, out string lobbyName, out string error))
            {
                ShowValidationError(error);
                return;
            }

            ShowValidationError(string.Empty);
            CreateLobbyName.text = lobbyName;
            TMMStoneLobby.Instance.CreateLobby(PrivacyToggle.isOn, lobbyName);

        });
        JoinByCodeButton.onClick.AddListener(() =>
        {
            string code = JoinCodeField.text.Trim().ToUpperInvariant();
            if (code.Length == 0 || !IsAlphaNumeric(code))
            {
                ShowValidationError("Enter a valid lobby code containing only letters and numbers.");
                return;
            }

            ShowValidationError(string.Empty);
            JoinCodeField.text = code;
            TMMStoneLobby.Instance.JoinLobbybyCode(code);
        });
        QuickJoinButton.onClick.AddListener(() =>
        {
            ShowValidationError(string.Empty);
            TMMStoneLobby.Instance.QuickJoin();
        });
        refreshButton.onClick.AddListener(RefreshButtonClick);
        TMMStoneLobby.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        TMMStoneLobby.Instance.OnLobbyUpdated += LobbyManager_OnLobbyUpdated;
    }
    private void LobbyManager_OnLobbyUpdated(object sender, TMMStoneLobby.LobbyUpdateEventArgs args)
    {
        if (args.lobby != null) JoinedLobbyUI.Instance.Show();
    }

    private void EnsureJoinControls()
    {
        if (JoinCodeField == null)
        {
            JoinCodeField = Instantiate(CreateLobbyName, CreateLobbyName.transform.parent);
            JoinCodeField.name = "JoinLobbyCode";
            JoinCodeField.text = string.Empty;
            JoinCodeField.characterLimit = 16;
            JoinCodeField.GetComponent<RectTransform>().anchoredPosition += Vector2.down * 70f;
            JoinCodeField.gameObject.SetActive(true);
            TMP_Text placeholder = JoinCodeField.placeholder as TMP_Text;
            if (placeholder != null) placeholder.text = "Enter lobby code...";
        }

        if (JoinByCodeButton == null)
        {
            JoinByCodeButton = CreateButtonCopy("JoinByCode", "Join by code", 70f);
        }

        if (QuickJoinButton == null)
        {
            QuickJoinButton = CreateButtonCopy("QuickJoin", "Quick join", 140f);
        }

        if (ValidationMessage == null)
        {
            TextMeshProUGUI source = CreateLobbyButton.GetComponentInChildren<TextMeshProUGUI>();
            ValidationMessage = Instantiate(source, transform);
            ValidationMessage.name = "LobbyValidationMessage";
            ValidationMessage.text = string.Empty;
            ValidationMessage.color = Color.red;
            ValidationMessage.richText = false;
            RectTransform rect = ValidationMessage.rectTransform;
            rect.anchoredPosition = Vector2.down * 100f;
            rect.sizeDelta = new Vector2(700f, 60f);
        }
    }

    private Button CreateButtonCopy(string objectName, string label, float yOffset)
    {
        Button button = Instantiate(CreateLobbyButton, CreateLobbyButton.transform.parent);
        button.name = objectName;
        button.onClick.RemoveAllListeners();
        button.GetComponent<RectTransform>().anchoredPosition += Vector2.down * yOffset;
        button.gameObject.SetActive(true);
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.text = label;
        return button;
    }

    private void ShowValidationError(string message)
    {
        ValidationMessage.text = message;
    }

    public static bool TryValidateLobbyName(string input, out string normalizedName, out string error)
    {
        normalizedName = (input ?? string.Empty).Normalize(NormalizationForm.FormC).Trim();
        StringBuilder collapsed = new();
        bool previousWasSpace = false;

        foreach (char character in normalizedName)
        {
            if (character == ' ')
            {
                if (!previousWasSpace) collapsed.Append(character);
                previousWasSpace = true;
                continue;
            }

            previousWasSpace = false;
            UnicodeCategory category = char.GetUnicodeCategory(character);
            bool isLetterOrNumber = category == UnicodeCategory.UppercaseLetter
                || category == UnicodeCategory.LowercaseLetter
                || category == UnicodeCategory.TitlecaseLetter
                || category == UnicodeCategory.ModifierLetter
                || category == UnicodeCategory.OtherLetter
                || category == UnicodeCategory.NonSpacingMark
                || category == UnicodeCategory.SpacingCombiningMark
                || category == UnicodeCategory.EnclosingMark
                || category == UnicodeCategory.DecimalDigitNumber
                || category == UnicodeCategory.LetterNumber
                || category == UnicodeCategory.OtherNumber;
            if (!isLetterOrNumber && character != '-' && character != '_' && character != '\'' && character != '’')
            {
                error = "Use letters, numbers, spaces, hyphens, underscores, or apostrophes only.";
                return false;
            }

            collapsed.Append(character);
        }

        normalizedName = collapsed.ToString();
        int length = new StringInfo(normalizedName).LengthInTextElements;
        if (length < 1 || length > 32)
        {
            error = "Lobby names must contain between 1 and 32 characters.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool IsAlphaNumeric(string value)
    {
        foreach (char character in value)
            if (!(character >= 'A' && character <= 'Z') && !(character >= '0' && character <= '9')) return false;
        return true;
    }
    public void Hide() => gameObject.SetActive(false);
    public void Show() => gameObject.SetActive(true);
    void RefreshButtonClick()
    {
        TMMStoneLobby.Instance.ListLobbies();
    }
    void LobbyManager_OnLobbyListChanged(object sender, TMMStoneLobby.LobbyEventArgs args)
    {
        UpdateLobbyList(args.lobbyList);
    }
    void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform lobby in container)
        {
            Destroy(lobby.gameObject);
        }


        foreach (Lobby lobby in lobbyList)
        {
            var instance = Instantiate(lobbyListingPrefab, container);
            LobbyListingUI ui = instance.GetComponent<LobbyListingUI>();
            Debug.Log(ui.ToString());
            ui.Initialize(lobby);
        }
    }
    /// <summary>
    /// Sanitizes input into valid C# classname
    /// ChatGPTied
    /// Its duplicated but its like in completely different assembly and I did not want to clean that up
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Sanitized input</returns>
    /// <exception cref="ArgumentException"></exception>
    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or empty.");
        }
        var sanitized = new StringBuilder();

        // Ensure the first character is a letter or underscore
        if (!char.IsLetter(input[0]) && input[0] != '_')
        {
            sanitized.Append('_');
        }

        foreach (var ch in input)
        {
            // Allow letters, digits, and underscores
            if (char.IsLetterOrDigit(ch) || ch == '_')
            {
                sanitized.Append(ch);
            }
            else
            {
                sanitized.Append('_');  // Replace invalid characters with underscores
            }
        }

        // Ensure it does not start with a digit (if it's not already handled)
        if (char.IsDigit(sanitized[0]))
        {
            sanitized.Insert(0, '_');
        }

        // Return sanitized string (e.g., "MyClassName123")
        return sanitized.ToString();
    }
    public void DeckBuilder()
    {
        SceneManager.LoadScene("DeckBuilderScene");
    }

}
