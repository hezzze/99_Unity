using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.U2D;

namespace NinetyNine
{
    public class UICard : MonoBehaviour
    {
        public SpriteAtlas Atlas;

        public Suits Suit = Suits.NoSuits;
        public Ranks Rank = Ranks.NoRanks;

        public string OwnerId = null;

        SpriteRenderer spriteRenderer;

        bool faceUp = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            UpdateSprite();
        }

        public void SetFaceUp(bool value)
        {
            faceUp = value;
            UpdateSprite();

            // TODO restore this logic for security 
            // Setting faceup to false also resets card's value.
            //if (value == false)
            //{
            //    Rank = Ranks.NoRanks;
            //    Suit = Suits.NoSuits;
            //}
        }

        public void SetCardData(byte value)
        {
            Rank = Card.GetRank(value);
            Suit = Card.GetSuit(value);
        }


        void UpdateSprite()
        {
            if (faceUp)
            {
                spriteRenderer.sprite = Atlas.GetSprite(SpriteName());
            }
            else
            {
                spriteRenderer.sprite = Atlas.GetSprite(Constants.CARD_BACK_SPRITE);
            }
        }

        string GetRankDescription()
        {
            FieldInfo fieldInfo = Rank.GetType().GetField(Rank.ToString());
            DescriptionAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes[0].Description;
        }

        string SpriteName()
        {
            if (Suit == Suits.NoSuits)
            {
                string testName = Rank == Ranks.BlackJoker ? "BJ" : "RJ";
                return $"card{testName}";
            }
            else
            {
                string testName = $"card{Suit}{GetRankDescription()}";
                return testName;
            }
        }

        public void SetDisplayingOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }

        public void OnSelected(bool selected)
        {
            if (selected)
            {
                transform.position = (Vector2)transform.position + Vector2.up * Constants.CARD_SELECTED_OFFSET;
            }
            else
            {
                transform.position = (Vector2)transform.position - Vector2.up * Constants.CARD_SELECTED_OFFSET;
            }
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}