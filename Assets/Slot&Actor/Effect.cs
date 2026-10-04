using System;
using UnityEngine;

namespace CardGame
{

    public class Effect : TableActor
    {
        public bool isExperiment = false;
        public int CardID { get; private set; }
        public string CardName { get; private set; }//TODO: Unneeded or to be placed into TableActor
        public void Destroy()
        {
            CardSlot slot = GetComponentInParent<CardSlot>();
            if (slot != null) backupOwner = slot.Owner;
            transform.SetParent(null);
            Destroy(gameObject);
        }
        public override void Initialize(Card c)
        {
            base.Initialize(c);
            CardID = c.ID;
            CardName = c.cardname;
            isExperiment = c.IsExperiment;
            if (isExperiment && Owner==GameManager.P.P2) {
                graphic.sprite = c.cardBack;
                CardArt.Load("card-face/card-back", sprite =>
                {
                    if (this != null && sprite != null) graphic.sprite = sprite;
                });
            }
        }
    }
}
