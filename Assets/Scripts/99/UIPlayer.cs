using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NinetyNine
{
    /// <summary>
    /// Manages the positions of the player's cards
    /// </summary>
    [Serializable]
    public class UIPlayer
    {
        public string PlayerId;
        public string PlayerName;
        public bool IsAI;
        public Vector2 Position;
        public Vector2 BookPosition;

        int numOfCardsInHand;
        //int numberOfBooks;

        public List<UICard> uihand = new List<UICard>();

        public GameObject panel; 

        public Vector2 NextCardPosition()
        {
            Vector2 nextPos = Position + Vector2.right * Constants.PLAYER_CARD_POSITION_OFFSET * numOfCardsInHand;
            return nextPos;
        }


        public void SetCardValues(List<byte> values)
        {
            if (uihand.Count != values.Count)
            {
                //Debug.LogError($"Displaying cards count {uihand.Count} is not equal to card values count {values.Count} for {PlayerId}");
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                UICard uicard = uihand[index];
                uicard.SetCardData(values[index]);
                uicard.SetDisplayingOrder(index);
            }
        }

        public void HideCardValues()
        {
            foreach (UICard uicard in uihand)
            {
                uicard.SetFaceUp(false);
            }
        }

        public void ShowCardValues()
        {
            foreach (UICard uicard in uihand)
            {
                uicard.SetFaceUp(true);
            }
        }

        public void ReceiveUICard(UICard uicard)
        {
            uihand.Add(uicard);
            uicard.OwnerId = PlayerId;
            numOfCardsInHand++;
        }

        //public void ReceiveBook(Ranks rank, CardAnimator cardAnimator)
        //{
        //    Vector2 targetPosition = NextBookPosition();
        //    List<Card> displayingCardsToRemove = new List<Card>();

        //    foreach (Card card in UICards)
        //    {
        //        if (card.Rank == rank)
        //        {
        //            card.SetFaceUp(true);
        //            float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
        //            cardAnimator.AddCardAnimation(card, targetPosition, Quaternion.Euler(Vector3.forward * randomRotation));
        //            displayingCardsToRemove.Add(card);
        //        }
        //    }

        //    UICards.RemoveAll(card => displayingCardsToRemove.Contains(card));
        //    RepositionDisplayingCards(cardAnimator);
        //    numberOfBooks++;
        //}


        //public void RestoreBook(Ranks rank, CardAnimator cardAnimator)
        //{
        //    Vector2 targetPosition = NextBookPosition();

        //    for (int i = 0; i < 4; i++)
        //    {
        //        Card card = cardAnimator.TakeFirstDisplayingCard();

        //        int intRankValue = (int)rank;
        //        int cardValue = (intRankValue - 1) * 4 + i;

        //        card.SetCardValue((byte)cardValue);
        //        card.SetFaceUp(true);
        //        float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
        //        card.transform.position = targetPosition;
        //        card.transform.rotation = Quaternion.Euler(Vector3.forward * randomRotation);
        //    }

        //    numberOfBooks++;
        //}

        public void RepositionDisplayingCards(CardDealer cardDealer)
        {
            numOfCardsInHand = 0;
            foreach (UICard uicard in uihand)
            {
                numOfCardsInHand++;
                cardDealer.AddCardAnimation(uicard, NextCardPosition());
            }
        }

        public void SendDisplayingCardToPlayer(UIPlayer receivingUIPlayer, CardDealer cardDealer, List<byte> cardValues, bool isLocalPlayer)
        {
            int playerDisplayingCardsCount = uihand.Count;

            if (playerDisplayingCardsCount < cardValues.Count)
            {
                Debug.LogError("Not enough displaying cards");
                return;
            }

            for (int index = 0; index < cardValues.Count; index++)
            {

                UICard uicard = null;
                byte cardValue = cardValues[index];

                if (isLocalPlayer)
                {
                    foreach (UICard c in uihand)
                    {
                        if (c.Rank == Card.GetRank(cardValue) && c.Suit == Card.GetSuit(cardValue))
                        {
                            uicard = c;
                            break;
                        }
                    }
                }
                else
                {
                    uicard = uihand[playerDisplayingCardsCount - 1 - index];
                    uicard.SetCardData(cardValue);
                    uicard.SetFaceUp(true);
                }

                if (uicard != null)
                {
                    uihand.Remove(uicard);
                    receivingUIPlayer.ReceiveUICard(uicard);
                    cardDealer.AddCardAnimation(uicard, receivingUIPlayer.NextCardPosition());
                    numOfCardsInHand--;
                }
                else
                {
                    Debug.LogError("Unable to find displaying card.");
                }
            }

            RepositionDisplayingCards(cardDealer);
        }


        // 99 stuff

        public void PlayCard(CardDealer cardDealer, UICard uicard)
        {
            Vector2 pos = (Vector2)uicard.transform.position + Vector2.up * Constants.PLAY_CARD_OFFSET;
            cardDealer.AddCardAnimation(uicard, pos);
            uicard.SetFaceUp(true);
            numOfCardsInHand--;
            uihand.Remove(uicard);
            uicard.OwnerId = null;

            cardDealer.playeddeck.Add(uicard);
            uicard.SetDisplayingOrder(cardDealer.nextPlayedOrderIdx());
            RepositionDisplayingCards(cardDealer);
        }

        public override string ToString()
        {
            string playerStats = $"\nplayer {PlayerId}\nhand: ";
            foreach (var uicard in uihand)
            {
                playerStats += $"{uicard} ";
            }
            playerStats += "\n";

            return playerStats;
        }

        public void SendACardToPlayer(UIPlayer receivingUIPlayer, CardDealer cardDealer, UICard uicard, bool faceUp)
        {
            uihand.Remove(uicard);
            receivingUIPlayer.ReceiveUICard(uicard);
            cardDealer.AddCardAnimation(uicard, receivingUIPlayer.NextCardPosition());
            uicard.SetFaceUp(faceUp);
            numOfCardsInHand--;

            RepositionDisplayingCards(cardDealer);
        }

        public void ShowPanel()
        {
            panel.SetActive(true);
        }


        //public bool Equals(UIPlayer other)
        //{
        //    if (player.pid.Equals(other.player.pid))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}
