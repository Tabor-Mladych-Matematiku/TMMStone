using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace CardGame
{
    public abstract class TableActor : GameActor
    {
        [SerializeField] SpriteRenderer HighlightRim;
        [SerializeField] SpriteRenderer graphic;
        Color highlightColor;
        Color defaultColor;
        private void Awake()
        {
            defaultColor = HighlightRim.color;
            highlightColor = new(defaultColor.r, defaultColor.g, defaultColor.b, 0.8f);
        }
        public void OnMouseEnter()
        {
            HighlightRim.color = highlightColor;
        }
        public void OnMouseExit()
        {
            HighlightRim.color = defaultColor;
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
            get
            {
                return h;
            }
            private set
            {
                h = value;
                HealthLabel.text = h.ToString();
                if (value <= 0) Death();
            }
        }

        private void Death()
        {
            OnDeath?.Invoke(this, new());
            transform.parent.GetComponent<MinionSlot>().RemoveMinion();
        }

        public int Attack
        {
            get
            {
                return a;
            }
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