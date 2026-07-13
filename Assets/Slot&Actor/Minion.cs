using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame
{
    public abstract class TableActor : GameActor
    {
        [SerializeField] protected Image HighlightRim;
        protected Color highlightColor;
        protected Color attkColor;
        protected Color defaultColor;
        [SerializeField] protected Image graphic;
        protected Card original;
        public List<Func<Card, int>> manacostmod = new();

        public virtual void OnMouseEnter()
        {
            if (GameManager.Instance.cursor == null)
            {
                original.HighlightCard();
            }
        }
        public virtual void OnMouseExit()
        {
            original.DeHighlightCard();
            HighlightRim.color = defaultColor;
            GameManager.Instance.highlightedActor = null;
        }
        public virtual void Initialize(Card c)
        {
            original = c;
            Sprite sprite = Resources.Load<Sprite>("CardPlainImages/" + c.expansion + "/" + c.cardname);
            if (sprite != null) graphic.sprite = sprite;
            expansion = c.expansion;
            cardTag = c.cardTag;
        }
    }
    public abstract class DamageableActor : TableActor
    {
        internal override void Awake() { base.Awake(); baseColor = graphic.color; }
        public abstract void Damage(int ammount);
        public abstract void Heal(int ammount);
        private bool f = false;
        Color frozenColor = new(0.1f, 0.1f, 1, 1);//This would/will be handled differently in the future
        Color baseColor;
        public bool Frozen
        {
            get => f; set
            {
                graphic.color = value ? frozenColor : baseColor;
                f = value;
            }
        }
    }

    public class Minion : DamageableActor
    {
        private int h = 0;
        private int a = 0;
        [SerializeField] TextMeshProUGUI AttackLabel;
        [SerializeField] TextMeshProUGUI HealthLabel;

        [SerializeField] AudioClip attackSound;

        public event EventHandler<TargetedEventEventArgs> OnSelfSummoned;
        public event EventHandler<TargetedEventEventArgs> OnBeforeAttack;
        public event EventHandler<TargetedEventEventArgs> OnAfterAttack;
        public event EventHandler OnDeath;
        public event EventHandler OnHealed;
        public event EventHandler OnDamaged;
        public class TargetedEventEventArgs
        {
            public GameActor target;
        }
        //Health on card
        private int baseHealth;
        private int mh;
        //Current max Health
        public int MaxHealth
        {
            get => mh; set
            {
                mh = value;
                h = math.min(h, mh);
            }
        }
        //Curent Health
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
        //On Silence and such
        public void ResetMaxHealth()
        {
            MaxHealth = baseHealth;
        }
        public override void Heal(int ammount)
        {
            if (ammount <0) throw new ArgumentException("Ammount must be greater or equal to 0");
            try
            {
                checked
                {
                    Health = math.min(Health + ammount, MaxHealth);
                }
            }
            catch (OverflowException)
            {
                Health = MaxHealth;
            }
            if (ammount > 0) OnHealed?.Invoke(this, new());
        }
        public override void Damage(int ammount)
        {
            if (ammount < 0) throw new ArgumentException("Ammount must be greater or equal to 0");
            Health = checked(Health - ammount);
            if (ammount > 0) OnDamaged?.Invoke(this, new());
        }
        internal override void Awake()
        {
            base.Awake();
            defaultColor = HighlightRim.color;
            highlightColor = new(defaultColor.r, defaultColor.g, defaultColor.b, 0.8f);
            attkColor = new(defaultColor.g, defaultColor.r, defaultColor.b, 0.8f);//Interesting choice but ok
        }
        public void Death()
        {
            OnDeath?.Invoke(this, new());
            GetComponentInParent<CardSlot>().RemoveMinion();
        }
        private int baseAttack;
        public int Attack
        {
            get => a;
            set
            {
                a = math.max(0, value);
                AttackLabel.text = a.ToString();
            }
        }
        public void ResetAttack()
        {
            Attack = baseAttack;
        }
        public void Buff(int attack, int health)
        {
            Attack += attack;
            MaxHealth += health;
            Health += health;
        }

        public bool CanAttack { get; private set; }
        public void Charge()
        {
            CanAttack = true;
        }


        public override void Initialize(Card c)
        {
            base.Initialize(c);
            Attack = c.stats[0];
            baseAttack = c.stats[0];
            Health = c.stats[1];
            MaxHealth = c.stats[1];
            baseHealth = c.stats[1];
            CanAttack = false;
        }

        public void OnMouseDown()
        {
            if (!GameManager.Instance.OnTurn || transform.parent.GetComponent<CardSlot>().Owner == GameManager.P.P2 || Attack == 0) return;//We must be on turn and we must be owner
            if (!CanAttack) return;//"That minion cannot attack yet!"
            GameManager.Instance.cursor = this;

        }
        public void OnMouseUp()
        {
            if (GameManager.Instance.cursor != this) return;
            if (GameManager.Instance.highlightedActor != null)
            {
                GameManager.Instance.OnUIMinionAttack(GetComponentInParent<CardSlot>().index, GameManager.Instance.HighlightedActorIndex);
            }
            transform.localPosition = Vector3.zero;
            GameManager.Instance.cursor = null;

        }
        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            if (GameManager.Instance.cursor != null)
            {
                //This is being targetted by spell
                if (GameManager.Instance.cursor is Card h && h.cardType == Card.CardType.Spell && h.Targetted && h.IsTargetValid(this))
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
            //Can this attack someone?
            else if (GameManager.Instance.OnTurn && Owner == GameManager.P.P1 && CanAttack && Attack != 0)
                HighlightRim.color = highlightColor;
        }

        public bool IsTargetValid(TableActor target)
        {
            return true; //TODO taunt and such
        }
        public bool CanAwake()
        {
            if (Frozen)
            {
                Frozen = false;
                return false;
            }
            return true;
        }
        public override void StartTurn(bool onTurn)
        {
            base.StartTurn(onTurn);
            if (GameManager.Instance.PlayerOnTurn == Owner && CanAwake()) CanAttack = true;//Cannot use onTurn cuz we also need to know if the minion is owned by the player who is on turn
        }

        public void AttackAction(Minion oppminion)
        {
            OnBeforeAttack?.Invoke(this, new() { target = oppminion });
            CanAttack = false;//TODO windfury shit.
            Damage(oppminion.Attack);
            oppminion.Damage(Attack);
            audioSource.PlayOneShot(attackSound);
            OnAfterAttack?.Invoke(this, new() { target = oppminion });
            //TODO visuals
        }
        public void AttackAction(Face face)
        {
            OnBeforeAttack?.Invoke(this, new() { target = face });
            CanAttack = false;//TODO windfury shit.
            face.Damage(Attack);
            audioSource.PlayOneShot(attackSound);
            OnAfterAttack?.Invoke(this, new() { target = face });
        }

        public bool Alive()
        {
            return Health > 0;
        }

        internal void Summoned(GameActor target)
        {
            OnSelfSummoned?.Invoke(this, new() { target = target });
        }
    }
}