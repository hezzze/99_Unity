using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NinetyNine
{
    public class CardAnimation
    {
        UICard uicard;
        Vector2 destination;
        Quaternion rotation;

        public CardAnimation(UICard c, Vector2 pos)
        {
            uicard = c;
            destination = pos;
            rotation = Quaternion.identity;
        }

        public CardAnimation(UICard c, Vector2 pos, Quaternion rot)
        {
            uicard = c;
            destination = pos;
            rotation = rot;
        }

        public bool Play()
        {
            bool finished = false;

            if (Vector2.Distance(uicard.transform.position, destination) < Constants.CARD_SNAP_DISTANCE)
            {
                uicard.transform.position = destination;
                finished = true;
            }
            else
            {
                uicard.transform.position = Vector2.MoveTowards(uicard.transform.position, destination, Constants.CARD_MOVEMENT_SPEED * Time.deltaTime);
                uicard.transform.rotation = Quaternion.Lerp(uicard.transform.rotation, rotation, Constants.CARD_ROTATION_SPEED * Time.deltaTime);
            }

            return finished;
        }
    }

    /// <summary>
    /// Controls all card animations in the game
    /// </summary>
    public class CardDealer : MonoBehaviour
    {
        [SerializeField]
        protected GameObject CardPrefab;

        public List<UICard> uideck;

        Queue<CardAnimation> cardAnimations;

        CardAnimation currentCardAnimation;

        Vector2 startPosition = new Vector2(-5f, 1f);

        // invoked when all queued card animations have been played
        public UnityEvent OnAllAnimationsFinished = new UnityEvent();

        bool working = false;


        public List<UICard> playeddeck = new List<UICard>();
        public byte playedOrderIdx = 0;


        void Awake()
        {
            cardAnimations = new Queue<CardAnimation>();
        }

        // called from game controller to setup UI cards
        public void InitializeUIDeck(List<byte> values)
        {
            uideck = new List<UICard>();

            for (byte i = 0; i < values.Count; i++)
            {
                Vector2 newPosition = startPosition + Vector2.right * Constants.DECK_CARD_POSITION_OFFSET * i;
                GameObject newGameObject = Instantiate(CardPrefab, newPosition, Quaternion.identity);
                newGameObject.transform.parent = transform;
                UICard uicard = newGameObject.GetComponent<UICard>();

                // associate card data with each ui card 
                uicard.SetCardData(values[i]);

                uicard.SetDisplayingOrder(i);
                uicard.transform.position = newPosition;
                uideck.Add(uicard);
            }
        }

        public UICard TakeFirstDisplayingCard()
        {
            int numberOfDisplayingCard = uideck.Count;

            if (numberOfDisplayingCard > 0)
            {
                UICard uicard = uideck[numberOfDisplayingCard - 1];
                uideck.Remove(uicard);

                return uicard;
            }

            return null;
        }

        public void DealDisplayingCards(UIPlayer uiplayer, int numberOfCard, bool animated = true)
        {
            int start = uideck.Count - 1;
            int finish = uideck.Count - 1 - numberOfCard;

            List<UICard> cardsToRemoveFromDeck = new List<UICard>();

            for (int i = start; i > finish; i--)
            {
                UICard uicard = uideck[i];
                uiplayer.ReceiveUICard(uicard);
                cardsToRemoveFromDeck.Add(uicard);
                if (animated)
                {
                    AddCardAnimation(uicard, uiplayer.NextCardPosition());
                }
                else
                {
                    uicard.transform.position = uiplayer.NextCardPosition();
                }
            }

            foreach (UICard uicard in cardsToRemoveFromDeck)
            {
                uideck.Remove(uicard);
            }
        }

        public void DrawDisplayingCard(UIPlayer uiplayer)
        {
            int numberOfDisplayingCard = uideck.Count;

            if (numberOfDisplayingCard > 0)
            {
                UICard uicard = uideck[numberOfDisplayingCard - 1];
                uiplayer.ReceiveUICard(uicard);
                AddCardAnimation(uicard, uiplayer.NextCardPosition());

                uideck.Remove(uicard);
            }
        }

        public void DrawDisplayingCard(UIPlayer uiplayer, byte value)
        {
            int numberOfDisplayingCard = uideck.Count;

            if (numberOfDisplayingCard > 0)
            {
                UICard uicard = uideck[numberOfDisplayingCard - 1];
                uicard.SetCardData(value);
                uicard.SetFaceUp(true);
                uiplayer.ReceiveUICard(uicard);
                AddCardAnimation(uicard, uiplayer.NextCardPosition());

                uideck.Remove(uicard);
            }
        }

        public void AddCardAnimation(UICard uicard, Vector2 position)
        {
            CardAnimation ca = new CardAnimation(uicard, position);
            cardAnimations.Enqueue(ca);
            working = true;
        }

        public void AddCardAnimation(UICard uicard, Vector2 position, Quaternion rotation)
        {
            CardAnimation ca = new CardAnimation(uicard, position, rotation);
            cardAnimations.Enqueue(ca);
            working = true;
        }

        private void Update()
        {
            if (currentCardAnimation == null)
            {
                NextAnimation();
            }
            else
            {
                if (currentCardAnimation.Play())
                {
                    NextAnimation();
                }
            }
        }

        void NextAnimation()
        {
            currentCardAnimation = null;

            if (cardAnimations.Count > 0)
            {
                CardAnimation ca = cardAnimations.Dequeue();
                currentCardAnimation = ca;
            }
            else
            {
                if (working)
                {
                    working = false;
                    OnAllAnimationsFinished.Invoke();
                }
            }
        }


        // 99 stuff

        public void DealCardToPlayer(UIPlayer uiplayer, Ranks rank, Suits suit, bool hide = false, bool animated = true)
        {
            int deckSize = uideck.Count;

            if (deckSize > 0)
            {
                UICard uicard = uideck.Find(c => c.Rank == rank && c.Suit == suit);
                uiplayer.ReceiveUICard(uicard);
                AddCardAnimation(uicard, uiplayer.NextCardPosition());
                if (hide) uicard.SetFaceUp(false);
                uideck.Remove(uicard);
            }
        }


        public void refillDeck(List<byte> values)
        {
            foreach(var uicard in playeddeck)
            {
                Destroy(uicard.gameObject);
            }
            
            playeddeck = new List<UICard>();
            playedOrderIdx = 0;

            InitializeUIDeck(values);
        }

        public byte nextPlayedOrderIdx ()
        {
            return playedOrderIdx++;
        }
    }
}

