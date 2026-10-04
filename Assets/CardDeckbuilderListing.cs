using CardData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CardGame;

[RequireComponent(typeof(Image))]
public class CardDeckbuilderListing : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const float PreviewHeight = 600f;

    int cardID;
    Image cardImage;
    Image previewImage;
    public string CardClass { get; private set; }

    public CardDeckbuilderListing Instantiate(int id, CardData.CardData cardData)
    {
        cardID = id;
        CardClass = cardData.Class;
        cardImage = GetComponent<Image>();
        if (CDJsonUtils.expansionMapping.ContainsKey(cardData.expansion))
        {
            string expansion = CDJsonUtils.expansionMapping[cardData.expansion];
            CardArt.Load(CardArt.FaceAddress(expansion, cardData.name), sprite =>
            {
                if (this == null || sprite == null) return;
                cardImage.sprite = sprite;
                if (previewImage != null) previewImage.sprite = sprite;
            });
        }
        return this;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (previewImage != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        canvas = canvas.rootCanvas;

        GameObject preview = new("Card Hover Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        preview.transform.SetParent(canvas.transform, false);
        preview.transform.SetAsLastSibling();

        previewImage = preview.GetComponent<Image>();
        previewImage.sprite = cardImage != null ? cardImage.sprite : GetComponent<Image>().sprite;
        previewImage.preserveAspect = true;
        previewImage.raycastTarget = false;

        RectTransform previewRect = preview.GetComponent<RectTransform>();
        float aspect = previewImage.sprite != null
            ? previewImage.sprite.rect.width / previewImage.sprite.rect.height
            : 0.715f;
        previewRect.sizeDelta = new Vector2(PreviewHeight * aspect, PreviewHeight);
        previewRect.anchorMin = previewRect.anchorMax = new Vector2(0.5f, 0.5f);
        previewRect.pivot = new Vector2(0.5f, 0.5f);

        PositionPreview(canvas, previewRect, eventData.position, eventData.enterEventCamera);
    }

    public void OnPointerExit(PointerEventData eventData) => HidePreview();

    void OnDisable() => HidePreview();

    void OnDestroy() => HidePreview();

    private static void PositionPreview(Canvas canvas, RectTransform previewRect, Vector2 screenPosition, Camera eventCamera)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, eventCamera, out Vector2 localPoint))
            return;

        Vector2 halfSize = previewRect.sizeDelta * 0.5f;
        localPoint.x += halfSize.x + 24f;
        localPoint.x = Mathf.Clamp(localPoint.x, canvasRect.rect.xMin + halfSize.x, canvasRect.rect.xMax - halfSize.x);
        localPoint.y = Mathf.Clamp(localPoint.y, canvasRect.rect.yMin + halfSize.y, canvasRect.rect.yMax - halfSize.y);
        previewRect.anchoredPosition = localPoint;
    }

    private void HidePreview()
    {
        if (previewImage == null) return;
        Destroy(previewImage.gameObject);
        previewImage = null;
    }
}
