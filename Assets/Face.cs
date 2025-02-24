using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame
{
    public class Face : TableActor//We may wanna do some class Targettable or something. Its too similar to Minions and yet I don't want to do deeper inheritance from TableActors
    {
        public GameManager.P o { get;  set; }
        public override GameManager.P Owner
        {
            get {
                return o;
            }
        }
        public override void Initialize(Card c)
        {
            //base.Initialize(c); don't. This does nothing until we figure our Hero cards. WHICH WE ARE NOT DOING NOW
        }
        internal override void Awake()
        {
            base.Awake();
            defaultColor = HighlightRim.color;
            highlightColor = new(defaultColor.r, defaultColor.g, defaultColor.b, 0.8f);
            attkColor = new(defaultColor.g, defaultColor.r, defaultColor.b, 0.8f);
        }
        public override void OnMouseEnter()
        {
            if (GameManager.Instance.cursor == null) return;

            //This is being targetted by spell
            if (GameManager.Instance.cursor is Card h && h.cardType == Card.CardType.Spell && h.Targetted && h.IsTargetValid(this))// && transform.childCount == 0) Idk this was here but it makes little sense.
            {
                GameManager.Instance.highlightedActor = this;
                HighlightRim.color = highlightColor;
            }
            //This is being targetted by attack
            else if (GameManager.Instance.cursor is Minion m && Owner == GameManager.P.P2 && m.IsTargetValid(this))
            {
                GameManager.Instance.highlightedActor = this;
                HighlightRim.color = attkColor;
            }

        }
        public override void OnMouseExit()
        {
            HighlightRim.color = defaultColor;
            GameManager.Instance.highlightedActor = null;
        }
    }
}