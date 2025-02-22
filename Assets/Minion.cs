using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;

namespace CardGame
{
    public abstract class TableActor : GameActor
    {
        [SerializeField] protected SpriteRenderer HighlightRim;
        protected Color highlightColor;
        protected Color defaultColor;
        [SerializeField] SpriteRenderer graphic;
        public virtual void OnMouseEnter()
        {
        }
        public virtual void OnMouseExit()
        {
            
        }
        public virtual void Initialize(Card c)
        {
            Sprite sprite = Resources.Load<Sprite>("CardPlainImages/" + c.expansion + "/" + c.cardname);
            if (sprite != null) graphic.sprite = sprite;
        }
    }

    public class Minion : TableActor
    {
        private int h;
        private int a;
        [SerializeField] TextMesh AttackLabel;
        [SerializeField] TextMesh HealthLabel;


        public event EventHandler<BattlecryEventArgs> OnBattleCry;
        public event EventHandler OnDeath;
        public class BattlecryEventArgs
        {
            public int target;
        }
        public int Health
        {
            get => h;
            private set
            {
                h = value;
                HealthLabel.text = h.ToString();
                if (value <= 0) Death();
            }
        }
        private void Awake()
        {
            defaultColor = HighlightRim.color;
            highlightColor = new(defaultColor.r, defaultColor.g, defaultColor.b, 0.8f);
        }
        private void Death()
        {
            OnDeath?.Invoke(this, new());
            transform.parent.GetComponent<MinionSlot>().RemoveMinion();
        }

        public int Attack
        {
            get=> a;
            private set
            {
                a = math.max(0, value);
                AttackLabel.text = a.ToString();
            }
        }

        public bool CanAttack { get; private set; }


        public override void Initialize(Card c)
        {
            base.Initialize(c);
            Attack = c.stats[0];
            Health = c.stats[1];
            CanAttack = false;
        }

        public void OnMouseDown()
        {
            if (!GameManager.Instance.OnTurn || transform.parent.GetComponent<MinionSlot>().Owner == GameManager.P.P2) return;//We must be on turn and we must be owner
            if (!CanAttack) return;

        }
        public override void OnMouseEnter()
        {
            if (GameManager.Instance.OnTurn && Owner == GameManager.P.P1)
                HighlightRim.color = highlightColor;
            Card holding = GameManager.Instance.cursor;
            if (holding != null && holding.cardType == Card.CardType.Spell && holding.Targetted && holding.IsTargetValid(this) && transform.childCount == 0)
            {
                GameManager.Instance.highlightedActor = this;
            }
        }
        public override void OnMouseExit()
        {
            HighlightRim.color = defaultColor;
            GameManager.Instance.highlightedActor = null;
        }
        internal void Battlecry(int target)
        {
            OnBattleCry?.Invoke(this, new() { target = target });
        }
        public bool CanAwake()
        {
            return true; // Freeze effects and the sort
        }
        public override void StartTurn(bool onTurn)
        {
            base.StartTurn(onTurn);
            if (CanAwake()) CanAttack = true;
        }
    }
}