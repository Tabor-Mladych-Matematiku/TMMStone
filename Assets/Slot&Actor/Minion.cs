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
        protected Color attkColor;
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
        private int h = 0;
        private int a = 0;
        [SerializeField] TextMesh AttackLabel;
        [SerializeField] TextMesh HealthLabel;


        public event EventHandler<TargetedEventEventArgs> OnBattleCry;
        public event EventHandler<TargetedEventEventArgs> OnAttack;
        public event EventHandler OnDeath;
        public event EventHandler OnHealed;
        public event EventHandler OnDamaged;
        public class TargetedEventEventArgs
        {
            public int target;
        }
        public int Health
        {
            get => h;
            private set
            {
                if (h < value && h != 0) OnHealed?.Invoke(this, new());//the !=0 should dodge the init phase
                else OnDamaged?.Invoke(this, new());
                h = value;
                HealthLabel.text = h.ToString();
                if (value <= 0) Death();
            }
        }
        private void Awake()
        {
            defaultColor = HighlightRim.color;
            highlightColor = new(defaultColor.r, defaultColor.g, defaultColor.b, 0.8f);
            attkColor = new(defaultColor.g, defaultColor.r, defaultColor.b, 0.8f);//Interesting choice but ok
        }
        private void Death()
        {
            OnDeath?.Invoke(this, new());
            transform.parent.GetComponent<CardSlot>().RemoveMinion();
        }

        public int Attack
        {
            get => a;
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
            if (!GameManager.Instance.OnTurn || transform.parent.GetComponent<MinionSlot>().Owner == GameManager.P.P2 || Attack==0) return;//We must be on turn and we must be owner
            if (!CanAttack) return;//"That minion cannot attack yet!"
            GameManager.Instance.cursor = this;

        }
        public void OnMouseUp()
        {
            if(GameManager.Instance.cursor == this)
            {
                if (GameManager.Instance.highlightedActor != null)
                {
                    GameManager.Instance.OnUIAttackMinion(GetComponentInParent<CardSlot>().index, GameManager.Instance.HighlightedActorIndex);
                    CanAttack = false;//TODO windfury shit.
                }
                transform.localPosition = new Vector3(0, 0, -1);
                GameManager.Instance.cursor = null;
            }
        }
        public override void OnMouseEnter()
        {
            if (GameManager.Instance.cursor != null)
            {
                //This is being targetted by spell
                if (GameManager.Instance.cursor is Card h && h.cardType == Card.CardType.Spell && h.Targetted && h.IsTargetValid(this))// && transform.childCount == 0) Idk this was here but it makes little sense.
                {
                    GameManager.Instance.highlightedActor = this;
                    HighlightRim.color = highlightColor;
                }
                //This is being targetted by attack
                else if (GameManager.Instance.cursor is Minion m && Owner==GameManager.P.P2 && m.IsTargetValid(this))
                {
                    GameManager.Instance.highlightedActor = this;
                    HighlightRim.color = attkColor;
                }
            }
            //Can this attack someone?
            else if (GameManager.Instance.OnTurn && Owner == GameManager.P.P1 && CanAttack && Attack!=0)
                HighlightRim.color = highlightColor;
        }

        private bool IsTargetValid(Minion minion)
        {
            return true; //TODO taunt and such
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

        internal void AttackAction(Minion oppminion)
        {
            OnAttack?.Invoke(this, new() { target = oppminion.GetComponentInParent<CardSlot>().index });
            Health -= oppminion.Attack;
            oppminion.Health -= Attack;
            //TODO visuals
        }
    }
}