using System;
using UnityEngine;

namespace CardGame
{

    public class Effect : TableActor
    {
        public bool isExperiment = false;
        public void Destroy()
        {
            Destroy(gameObject);
        }
        public override void Initialize(Card c)
        {
            base.Initialize(c);
            if (isExperiment && Owner==GameManager.P.P2) {
                graphic.sprite = c.cardBack;
            }
        }
    }
}