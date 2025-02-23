using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame
{
    public abstract class PlacableSlot : CardSlot
    {
        public abstract bool IsCardPlacable(Card c);
        Image HighlightBox;
        Color highlightc = new(0, 1, 0, 0.3f);

        public /*override*/ void Awake()
        {
            HighlightBox = GetComponent<Image>();
        }
        protected virtual void OnMouseOver()
        {
            if (GameManager.Instance.cursor != null && GameManager.Instance.cursor is Card holding && IsCardPlacable(holding) && transform.childCount == 0)
            {
                HighlightBox.color = highlightc;
                GameManager.Instance.highlightedSlot = transform;
            }
        }
        protected virtual void OnMouseExit()
        {
            if (Owner != GameManager.P.P1) return;
            HighlightBox.color = new(0, 0, 0, 0);
            GameManager.Instance.highlightedSlot = null;
        }
    }

    public class CardSlot : MonoBehaviour
    {
        public bool Occupied { get => transform.childCount != 0; }
        public Vector3 Position { get => Position; }
        public GameManager.P Owner { get; set; }
        AudioSource minionDestroyPlayer;
        public int index;
        public virtual void Start()
        {
            minionDestroyPlayer = gameObject.AddComponent<AudioSource>();
            minionDestroyPlayer.clip = Resources.Load<AudioClip>("Sounds/MinionDestroyed");
        }
        public void Initialize(GameManager.P owner, int index)
        {
            Owner = owner;
            this.index = index;
        }
        public void PlaceCard(Card c)
        {
            if (Occupied) throw new CardSlotException("Already occupied");
            c.transform.parent = transform;
        }
        public Card GetCard()=> transform.GetComponentInChildren<Card>();
        public Card PopCard()
        {
            Card c = transform.GetComponentInChildren<Card>(true);
            c.transform.parent = null;
            return c;
        }
        public Minion GetMinion()=> transform.GetComponentInChildren<Minion>();
        public void RemoveMinion()
        {
            Destroy(GetMinion().gameObject);
            Card card = PopCard();
            card.gameObject.SetActive(true);
            GameManager.Instance.AddToGrave(card, Owner);
            minionDestroyPlayer.Play();
        }
        public class CardSlotException : Exception
        {
            public CardSlotException(string message) : base(message) { }
        }
    }
}