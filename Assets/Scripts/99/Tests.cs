using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Game rule* /
 * 
 * 
 * Game Start: Game is played with a standard french deck shuffled, 5 cards are dealt
 * to each player at the beginning, and they take turns to play. Cards left will 
 * be the deck to draw from. Intial direction is clockwise. Initial points is 0. 
 * 
 * Each turn: turn player can play a single card, and based on the rank of the card, 
 * points will be changed or card will be drawn from other players, etc. The player 
 * should do their best to avoid the points to reach over 99, and also avoid to
 * have no card left in hand, either case will make the player go "Busted". 
 * 
 * Turn End: turn player can draw a new card, unless the card they played prevent 
 * them from doing so. All players who has no card left are busted at turn end. 
 * At turn end if there is 0 card left in the stock pile after last draw, collect 
 * all played cards, shuffle them to form a new deck for drawing.
 * 
 * 
 * Game Over: At Turn End, if there is only one player 
 * (or in group fight only one group are left not busted)
 * that is still not "Busted, the game ends with that player (group) to be the 
 * winner of the game.
 * 
 * Normal Cards
 * 2,3,5,6,8,9 => +n
 * these are normal cards, and what they do is add face value to the points
 * (only adding, not substracting)
 * 
 * Special Cards (no draw card at turn end for cards marked with (*))
 * A: choose a player to play in next turn
 * 4: value is 0 and the order of play is reversed. has no effect if there is 
 * only two players left
 * 7: exchange hand with a player (*)
 * 10: value is -10 or +10 (player chooses).
 * J: choose a player and draw a card from the player's hand (*)
 * Q: value is -20 or +20 (player chooses)
 * K: make the points to be 99.
 * Red Joker: Revive a player (*)
 * Black Joker: Make a player busted immediately (*)
 * 
 *
 *
 */

namespace NinetyNine
{
    public class Tests : MonoBehaviour
    {

        static string[] testPids = new string[] { "P1", "P2", "P3", "P4" };

        // Start is called before the first frame update
        void Awake()
        {
            RunTests();
        }

        // Update is called once per frame
        void Update()
        {

        }


        void RunTests()
        {
            Test1();
            Test2();
            Test3();
            Test4();
            Test5();
            Test6();
            Test7();
            Test8();
            Test9();
            Test10();

            TestJack1();
            TestJack2();
            TestJack3();

            TestQueen1();
            TestQueen2();

            TestTen1();
            TestTen2();

            TestAce1();
            TestAce2();
            TestAce3();
            TestAce4();

            TestCard();

            Debug.Log("All tests passed");
        }

        //****************** Unit Tests *********************//

        void Test1()
            // player cannot play if it's not their turn
        {
            var testState = getNewTestState(0);

            CheckException(1, new System.Exception("WRONG_TURN"), () =>
            {
                var players = testState.GetPlayers();
                testState.makeMove(players[1].pid, new Card(0), players[0].pid, null);
            });
        }

        void Test2()
            // cannot play a card if not in player's hand
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            players[0].cards.Add(new Card(3));

            CheckException(2, new System.Exception("CARD_NOT_FOUND"), () =>
            {
                var players = testState.GetPlayers();
                testState.makeMove(players[0].pid, new Card(Ranks.Ace, Suits.Clubs), players[1].pid, null);
            });
        }

        void Test3()
            // play 3 of spades, points increase 3, turn updated
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            players[0].cards.Add(new Card(Ranks.Three, Suits.Spades));

            testState.makeMove(players[0].pid, new Card(8), players[1].pid, null);

            Assert(3, testState.GetPoints() == 3);
            Assert(3, testState.GetTurn() == players[1]);
        }

        void Test4()
            // play 3 of spades when points is 99, player dead
        {
            var testState = getNewTestState(99);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            players[0].cards.Add(new Card(Ranks.Three, Suits.Spades));
            players[0].cards.Add(new Card(Ranks.Eight, Suits.Spades));

            testState.makeMove(players[0].pid, new Card(8), players[1].pid, null);

            Assert(4, players[0].alive == false);
            Assert(4, testState.GetPoints() == 99);
        }

        void Test5()
            // play 4 of spades, direction switched
        {
            var testState = getNewTestState(99);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            players[0].cards.Add(new Card(Ranks.Four, Suits.Spades));

            testState.makeMove(players[0].pid, new Card(12), players[1].pid, null);

            Assert(5, testState.GetTurn() == players[3]);
        }

        void Test6()
            // played card go to used, player draw a new card 
        {
            var testState = getNewTestState(99);

            var players = testState.GetPlayers();

            var originCount = players[0].cards.Count;
            var cardToPlay = players[0].cards[0];
            int expectedCount;

            // TODO split this into two deterministic tests
            switch (cardToPlay.Rank)
            {
                case Ranks.Seven:
                case Ranks.RedJoker:
                case Ranks.BlackJoker:
                    // 7, RJ, BJ cannot draw
                    // J will draw a card from other player 
                    expectedCount = originCount - 1;
                    break;
                default:
                    expectedCount = originCount;
                    break;
            }

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, new How(true, 0));

            Assert("6.1", players[0].cards.Count == expectedCount);
            Assert("6.2", testState.GetUsed().Contains(cardToPlay));

        }

        void Test7()
            // used card get reshuffled and form a new unused deck
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.King, Suits.Hearts);
            players[0].cards.Add(cardToPlay);

            List<Card> unused = testState.GetUnUsed();
            unused.Clear();
            unused.Add(new Card(Ranks.BlackJoker, Suits.NoSuits));

            List<Card> used = testState.GetUsed();
            used.Clear();

            Card card1 = new Card(Ranks.Nine, Suits.Spades);
            Card card2 = new Card(Ranks.Two, Suits.Hearts);
            Card card3 = new Card(Ranks.Eight, Suits.Clubs);
            Card card4 = new Card(Ranks.Queen, Suits.Diamonds);

            used.Add(card1);
            used.Add(card2);
            used.Add(card3);
            used.Add(card4);

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, null);

            List<Card> unusedAfter = testState.GetUnUsed();

            Assert(7, unusedAfter.Count == 5);
            Assert(7, unusedAfter.Contains(cardToPlay));
            Assert(7, unusedAfter.Contains(card1));
            Assert(7, unusedAfter.Contains(card2));
            Assert(7, unusedAfter.Contains(card3));
            Assert(7, unusedAfter.Contains(card4));
        }

        void Test8()
            // after move one player busted, left only 1 player alive, game ends
        {
            var testState = getNewTestState(99);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Three, Suits.Spades);
            players[0].cards.Add(cardToPlay);

            foreach (var p in players)
            {
                p.alive = false;
            }

            players[0].alive = true;
            players[1].alive = true;

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, null);

            Assert(8, players[0].alive == false);
            Assert(8, testState.GetGameOver());
            Assert(8, testState.GetWinner() == players[1]);
        }

        void Test9()
            // last Turn returns the last turn player
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();
            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Two, Suits.Spades);
            players[0].cards.Add(cardToPlay);

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, null);

            Assert(9, testState.GetLastTurnId() == players[0].pid);
        }

        void Test10()
            // last draw returns the last played card
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();
            var cardToPlay = players[0].cards[0];

            var unused = testState.GetUnUsed();
            Card expectedLastDraw = unused[unused.Count - 1];

            // TODO split this into two deterministic tests
            switch (cardToPlay.Rank)
            {
                case Ranks.Seven:
                case Ranks.Jack:
                case Ranks.RedJoker:
                case Ranks.BlackJoker:
                    // 7, J, RJ, BJ cannot draw 
                    expectedLastDraw = null;
                    break;
            }

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, new How(true, 0));

            Assert(10, testState.GetLastDraw() == expectedLastDraw);
        }

        



        void TestJack1()
        // draw index needs to be valid
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Jack, Suits.Spades);
            players[0].cards.Add(cardToPlay);

            CheckException("Jack1", new System.Exception("DRAW_A_CARD_ERROR"), () =>
            {
                testState.makeMove(players[0].pid, cardToPlay, players[1].pid, new How(null, players[1].cards.Count));
            });


        }

        void TestJack2()
        // play jack of spades, draw a card from another player 
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Jack, Suits.Spades);
            players[0].cards.Add(cardToPlay);
            int toDrawIdx = players[1].cards.Count - 1;
            Card cardToDraw = players[1].cards[toDrawIdx];

            int countExpected0 = players[0].cards.Count;
            int countExpected1 = players[1].cards.Count - 1;


            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, new How(null, toDrawIdx));

            // cannot draw when playing jack
            Assert("Jack2 - 1", players[0].cards.Count == countExpected0);

            Assert("Jack2 - 2", players[0].cards.Contains(cardToDraw));
            Assert("Jack2 - 3", players[1].cards.Count == countExpected1);

        }

        void TestJack3()
        // play jack of hearts, another player will be busted if only has one card, 
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Jack, Suits.Hearts);
            players[0].cards.Add(cardToPlay);

            players[1].cards.Clear();
            players[1].cards.Add(new Card(Ranks.Ace, Suits.Diamonds));

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, new How(null, 0));

            Assert("Jack3", players[1].alive == false);
        }

        void TestQueen1()
            // play queen, select plus to add 20
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Queen, Suits.Spades);
            players[0].cards.Add(cardToPlay);

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, new How(false, null));

            Assert("Queen1 - 1", testState.GetPoints() == 20);
            Assert("Queen1 - 2", testState.GetLastDraw() != null);
        }

        void TestQueen2()
        // play queen, select minus to substract 20
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Queen, Suits.Hearts);
            players[0].cards.Add(cardToPlay);

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, new How(true, null));

            Assert("Queen2 - 1", testState.GetPoints() == -20);
            Assert("Queen2 - 2", testState.GetLastDraw() != null);
        }

        void TestTen1()
        // play ten, select plus to add 10
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Ten, Suits.Clubs);
            players[0].cards.Add(cardToPlay);

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, new How(false, null));

            Assert("Ten1 - 1", testState.GetPoints() == 10);
            Assert("Ten1 - 2", testState.GetLastDraw() != null);
        }

        void TestTen2()
        // play ten, select minus to substract 10
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Ten, Suits.Diamonds);
            players[0].cards.Add(cardToPlay);

            testState.makeMove(players[0].pid, cardToPlay, players[1].pid, new How(true, null));

            Assert("Ten2 - 1", testState.GetPoints() == -10);
            Assert("Ten2 - 2", testState.GetLastDraw() != null);
        }

        void TestAce1()
            // play ace, choose the 2nd next player 
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Ace, Suits.Hearts);
            players[0].cards.Add(cardToPlay);

            testState.makeMove(players[0].pid, cardToPlay, players[2].pid, null);

            Assert("Ace1 - 1", testState.GetTurn() == players[2]);
        }

        void TestAce2()
            // when in a opposite direction, choose the next player, direction does not change
        {
            var testState = getNewTestState(0);

            testState.SetDirection(false);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Ace, Suits.Diamonds);
            players[0].cards.Add(cardToPlay);

            testState.makeMove(players[0].pid, cardToPlay, players[3].pid, null);

            Assert("Ace2 - 1", testState.GetTurn() == players[3]);
            Assert("Ace2 - 2", testState.GetDirection() == false);
        }

        void TestAce3()
        // cannot choose dead player
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Ace, Suits.Diamonds);
            players[0].cards.Add(cardToPlay);

            players[1].alive = false;

            CheckException("Ace3 - 1", new System.Exception("ILLEGAL_ACE_TARGET"), () =>
            {
                testState.makeMove(players[0].pid, cardToPlay, players[1].pid, null);
            });
        }


        void TestAce4()
        // play ace, choose the 2nd next player, skip 1 player
        {
            var testState = getNewTestState(0);

            var players = testState.GetPlayers();

            players[0].cards.Clear();
            var cardToPlay = new Card(Ranks.Ace, Suits.Hearts);
            players[0].cards.Add(cardToPlay);

            players[2].cards.Clear();
            var cardToPlay2 = new Card(Ranks.Three, Suits.Clubs);
            players[2].cards.Add(cardToPlay2);

            testState.makeMove(players[0].pid, cardToPlay, players[2].pid, null);
            testState.makeMove(players[2].pid, cardToPlay2, null, null);

            Assert("Ace4 - 1", testState.GetTurn() == players[3]);
        }

        void TestCard()
        {
            Assert("Card", new Card(Ranks.Four, Suits.Diamonds).value == 14);
            Assert("Card", new Card(Ranks.Seven, Suits.Hearts).value == 27);
            Assert("Card", new Card(Ranks.Jack, Suits.Spades).value == 40);
            Assert("Card", new Card(Ranks.RedJoker, Suits.NoSuits).value == 53);
            Assert("Card", new Card(Ranks.BlackJoker, Suits.NoSuits).value == 52);
            Assert("Card", new Card(Ranks.Nine, Suits.Clubs).value == 33);
        }




        void CheckException(int n, Exception expected, Action Act)
        {
            CheckException($"{n}", expected, Act);
        }

        void CheckException(string s, Exception expected, Action Act)
        {
            try
            {
                Act();
            }
            catch (Exception e)
            {
                if (e.Message != expected.Message)
                {
                    Debug.Log($"Expected: {expected} \n Actual: {e}");
                    throw new Exception($"Test {s} failed");
                }
            }
        }

        void Assert(int n, bool cond)
        {
            if (!cond)
            {
                throw new Exception($"Test {n} failed");
            }
        }

        void Assert(string s, bool cond)
        {
            if (!cond)
            {
                throw new Exception($"Test {s} failed");
            }
        }



        GameState getNewTestState(int points)
        {
            var deck = GameState.buildDeck();
            var players = new List<Player>();

            foreach (var p in testPids)
            {
                players.Add(new Player(p));
            }

            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < players.Count; j++)
                {
                    Card nextCard = deck[deck.Count - 1];
                    players[j].cards.Add(nextCard);
                    deck.Remove(nextCard);
                }
            }

            return new GameState(deck, players, points);
        }
    }
}

