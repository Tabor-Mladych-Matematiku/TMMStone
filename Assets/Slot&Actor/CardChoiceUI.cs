using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CardGame;

public static class CardChoiceUI
{
    public static void Show(CardChoiceRequest request, Action<int[]> resolve, Action cancel)
    {
        Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        GameObject overlay = NewUI("ChoiceOverlay", canvas.transform, typeof(Image), typeof(Button));
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        Stretch(overlayRect);
        Image overlayImage = overlay.GetComponent<Image>();
        Color overlayColor = new Color32(0, 0, 0, 237);
        overlayImage.color = overlayColor;
        Button overlayButton = overlay.GetComponent<Button>();
        overlayButton.transition = Selectable.Transition.None;
        bool choiceVisible = true;
        overlayButton.onClick.AddListener(() =>
        {
            if (!choiceVisible) return;
            UnityEngine.Object.Destroy(overlay);
            cancel();
        });

        GameObject content = NewUI("ChoiceContent", overlay.transform, typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(CanvasGroup));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = contentRect.anchorMax = contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        var vertical = content.GetComponent<VerticalLayoutGroup>();
        vertical.spacing = 20;
        vertical.childAlignment = TextAnchor.MiddleCenter;
        vertical.childControlWidth = vertical.childControlHeight = false;
        content.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI header = NewUI("Header", content.transform, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        header.text = request.Header;
        header.fontSize = 34;
        header.alignment = TextAlignmentOptions.Center;
        header.rectTransform.sizeDelta = new Vector2(1000, 60);

        GameObject grid = NewUI("Choices", content.transform, typeof(GridLayoutGroup));
        GridLayoutGroup layout = grid.GetComponent<GridLayoutGroup>();
        int rows = Mathf.CeilToInt(request.Options.Count / 5f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = Mathf.Max(1, Mathf.CeilToInt(request.Options.Count / (float)Mathf.Max(1, rows)));
        layout.cellSize = new Vector2(180, 250);
        layout.spacing = new Vector2(20, 20);
        grid.GetComponent<RectTransform>().sizeDelta = new Vector2(
            layout.constraintCount * 200, Mathf.Max(1, rows) * 270);

        List<int> selected = new();
        for (int i = 0; i < request.Options.Count; i++)
        {
            CardChoiceOption option = request.Options[i];
            GameObject item = NewUI("Choice", grid.transform, typeof(Image), typeof(Button), typeof(Outline), typeof(ChoiceHoverOutline));
            item.GetComponent<Image>().color = Color.white;
            Outline outline = item.GetComponent<Outline>();
            outline.effectColor = Color.green;
            outline.effectDistance = new Vector2(4, 4);
            outline.useGraphicAlpha = false;
            outline.enabled = false;
            TextMeshProUGUI label = NewUI("Label", item.transform, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            Stretch(label.rectTransform, 10);
            label.text = option.Text;
            label.color = Color.black;
            label.fontSize = 25;
            label.alignment = TextAlignmentOptions.Center;
            label.gameObject.SetActive(option.Text != null);
            if (option.Card != null)
                CardArt.Load(CardArt.FaceAddress(option.Card.expansion, option.Card.cardname), sprite =>
                { if (item != null && sprite != null) item.GetComponent<Image>().sprite = sprite; });

            TextMeshProUGUI marker = NewUI("Selected", item.transform, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            marker.text = "▲";
            marker.color = Color.red;
            marker.fontSize = 38;
            marker.alignment = TextAlignmentOptions.Center;
            marker.rectTransform.anchorMin = marker.rectTransform.anchorMax = new Vector2(0.5f, 0);
            marker.rectTransform.pivot = new Vector2(0.5f, 1);
            marker.rectTransform.anchoredPosition = new Vector2(0, -4);
            marker.rectTransform.sizeDelta = new Vector2(60, 45);
            marker.gameObject.SetActive(false);

            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (request.RequiredSelections == 1)
                {
                    UnityEngine.Object.Destroy(overlay);
                    resolve(new[] { option.Value });
                    return;
                }
                if (selected.Remove(option.Value)) marker.gameObject.SetActive(false);
                else { selected.Add(option.Value); marker.gameObject.SetActive(request.MarkSelections); }
                if (selected.Count == request.RequiredSelections)
                {
                    UnityEngine.Object.Destroy(overlay);
                    resolve(selected.ToArray());
                }
            });
        }

        GameObject toggleObject = NewUI("ToggleChoiceVisibility", overlay.transform, typeof(Image), typeof(Button));
        RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
        toggleRect.anchorMin = toggleRect.anchorMax = toggleRect.pivot = new Vector2(0.5f, 0.5f);
        toggleRect.sizeDelta = new Vector2(280, 55);
        float contentHeight = 60 + vertical.spacing + Mathf.Max(1, rows) * 270;
        toggleRect.anchoredPosition = new Vector2(0, -contentHeight / 2 - 45);

        TextMeshProUGUI toggleLabel = NewUI("Label", toggleObject.transform, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        Stretch(toggleLabel.rectTransform, 8);
        toggleLabel.text = "Skrýt volbu";
        toggleLabel.color = Color.black;
        toggleLabel.fontSize = 25;
        toggleLabel.alignment = TextAlignmentOptions.Center;

        CanvasGroup contentGroup = content.GetComponent<CanvasGroup>();
        toggleObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            choiceVisible = !choiceVisible;
            contentGroup.alpha = choiceVisible ? 1 : 0;
            contentGroup.interactable = choiceVisible;
            contentGroup.blocksRaycasts = choiceVisible;
            overlayImage.color = choiceVisible ? overlayColor : Color.clear;
            toggleLabel.text = choiceVisible ? "Skrýt volbu" : "Vrátit se k volbě";
        });
    }

    private static GameObject NewUI(string name, Transform parent, params Type[] components)
    {
        var go = new GameObject(name, typeof(RectTransform));
        foreach (Type component in components) go.AddComponent(component);
        go.transform.SetParent(parent, false);
        return go;
    }
    private static void Stretch(RectTransform rect, float inset = 0)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset); rect.offsetMax = new Vector2(-inset, -inset);
    }
}

public sealed class ChoiceHoverOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Outline outline;

    private void Awake() => outline = GetComponent<Outline>();
    public void OnPointerEnter(PointerEventData _) => outline.enabled = true;
    public void OnPointerExit(PointerEventData _) => outline.enabled = false;
}
