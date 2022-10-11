using System;
using System.Collections.Generic;


namespace NinetyNine
{
    public class GameState
    {
        int points;
        List<Card> used = new List<Card>();
        List<Card> unused;
        List<Player> players;
        Player turn;
        bool gameOver = false;
        int turnIdx = 0;

        Card lastDraw;
        Player lastTurn;
        Player winner = null;

        int chosenNextTurnIdx  = -1;
        

        // true is clockwise
        bool direction = true; 

        public GameState (List<Card> unused, List<Player> players, int points)
        {
            if (players.Count <= 1
                || unused.Count < 1
                || players.Exists(p => p.cards.Count == 0))
                throw new System.Exception("ILLEGAL_INIT_STATE");

            this.unused = unused;
            this.players = players;
            this.points = points;
            this.turn = players[0];
        }

        public static List<Card> buildDeck(int numOfCards = 54)
        {
            List<Card> deck = new List<Card>();

            List<byte> values = buildDeckValues(numOfCards);

            foreach (var v in values)
            {
                deck.Add(new Card(v));
            }

            return deck;
        }

        public static List<byte> buildDeckValues(int numOfCards = 54)
        {
            List<byte> cardValues = new List<byte>();
            Random rand = new Random();

            // 52 black joker, 53 red joker
            for (byte value = 0; value < numOfCards; value++)
            {
                cardValues.Add(value);
            }

            List<byte> shuffled = new List<byte>();

            for (int index = 0; index < numOfCards; index++)
            {
                int valueIndexToAdd = rand.Next(0, cardValues.Count);

                //UnityEngine.Debug.Log($"value to add: {valueIndexToAdd}");

                byte valueToAdd = cardValues[valueIndexToAdd];
                shuffled.Add(valueToAdd);
                cardValues.Remove(valueToAdd);
            }

            return shuffled;
        }


        void NextTurn()
        {
            // override next turn when chosen by Ace
            if (chosenNextTurnIdx >= 0)
            {
                turn = players[chosenNextTurnIdx];
                turnIdx = chosenNextTurnIdx;
                chosenNextTurnIdx = -1;
                return;
            }

            turnIdx = (turnIdx + (direction ? 1 : -1)) % players.Count;

            // set to last player
            if (turnIdx < 0) turnIdx = players.Count - 1;

            while(!players[turnIdx].alive)
            {
                turnIdx = (turnIdx + (direction ? 1 : -1)) % players.Count;
                if (turnIdx < 0) turnIdx = players.Count - 1;
            }
            turn = players[turnIdx];
        }

        void SetGameOver()
        {
            int liveCount = 0;
            Player lastAlive = null;
            foreach (var p in players)
            {
                if (p.alive)
                {
                    lastAlive = p;
                    liveCount++;
                }
            }

            gameOver = liveCount <= 1;
            if (gameOver) winner = lastAlive;

        }

        Card getNextCard()
        {
            Card nextCard = unused[unused.Count - 1];
            unused.RemoveAt(unused.Count - 1);

            lastDraw = nextCard;

            return nextCard;
        }


        public void makeMove(string pid, Card card, string targetId, How? how)
        {
            if (gameOver) throw new System.Exception("GAME_OVER");

            if (turn.pid != pid)
            {
                throw new System.Exception("WRONG_TURN");
            }

            //TODO
            // 1. Add logic to prevent target to be player itself
            // 2. target cannot be a dead player

            var player = turn;

            var target = players.Find(p => p.pid == targetId);

            var rank = card.Rank;
            var canDraw = true;

            var idx = player.cards.IndexOf(card);

            if (idx == -1)
            {
                throw new System.Exception("CARD_NOT_FOUND");
            } else
            {
                // remove the used card
                player.cards.RemoveAt(idx);
            }

            switch(rank)
            {
                case Ranks.Two:
                case Ranks.Three:
                case Ranks.Five:
                case Ranks.Six:
                case Ranks.Eight:
                case Ranks.Nine:
                    //TestDummy();

                    PlayNormal(card);
                    break;
                case Ranks.Ace:
                    //TestDummy();


                    PlayChooseNext(target);
                    break;
                case Ranks.Four:
                    PlayReverse();
                    break;
                case Ranks.Seven:
                    //TestDummy();


                    PlayExchange(player, target);
                    canDraw = false;
                    break;
                case Ranks.Ten:
                case Ranks.Queen:
                    //TestDummy();

                    // assume how.isSub is not null
                    Play1020(card, (bool)how.isSub);
                    break;
                case Ranks.Jack:
                    // assume how.drawCardIdx is not null
                    //TestDummy();

                    // move validation
                    int drawIdx = (int)how.drawCardIdx;

                    if (!(drawIdx >= 0 && drawIdx < target.cards.Count))
                    {
                        throw new System.Exception("DRAW_A_CARD_ERROR");
                    }

                    PlayDrawACard(player, target, (int)how.drawCardIdx);
                    canDraw = false;
                    break;
                case Ranks.King:
                    Play99();
                    break;
                case Ranks.BlackJoker:
                    //TestDummy();


                    PlayCurse(target);
                    canDraw = false;
                    break;
                case Ranks.RedJoker:
                    TestDummy();


                    //PlayRevive(target);
                    canDraw = false;
                    break;
                default:
                    throw new System.Exception("ILLEGAL_RANK_OR_SUIT");

            }

            if (canDraw)
            {
                Card c = getNextCard();

                // draw a new card
                player.cards.Add(c);

                //UnityEngine.Debug.Log($"{player.pid} drew ({c})");
            } else
            {
                // no card is drawn, set to null
                lastDraw = null;
            }
            

            used.Add(card);

            // if no card left in unused pile
            // shuffle used cards to rebuilt

            if (unused.Count == 0) ReshuffleUsed();



            // check points
            // see if current player is busted
            if (points > 99)
            {
                points = 99; // make sure points don't overflow
                player.alive = false;
            }

            // check all players and see if they're still alive
            foreach (var p in players)
            {
                if (p.cards.Count == 0) p.alive = false;
            }

            SetGameOver();

            lastTurn = turn;

            NextTurn();
            
        }

        void ReshuffleUsed()
        {
            int numInUsed = used.Count;
            
            for (int index = 0; index < numInUsed; index++)
            {
                Random rand = new Random();
                int ridx = rand.Next(0, used.Count);

                Card card = used[ridx];
                unused.Add(card);
                used.Remove(card);
            }
        }

        void TestDummy() {
            points += 0;
        }


        void PlayNormal(Card card)
        {
            //Debug.Log($"PlayNormal - card rank: {card.Rank}");
            
            points += card.Rank.GetHashCode();
        }

        void PlayChooseNext(Player target)
        {
            if (!target.alive) throw new System.Exception("ILLEGAL_ACE_TARGET");
            chosenNextTurnIdx = players.IndexOf(target);
        }

        void PlayReverse()
        {
            direction = !direction;
        }

        void PlayExchange(Player player, Player target)
        {
            var cards = player.cards;
            player.cards = target.cards;
            target.cards = cards;
        }

        void Play1020(Card card, bool isSub)
        {
            var deltaPoints = card.Rank == Ranks.Ten ? 10 : 20;
            points += (isSub ? -deltaPoints : deltaPoints);
        }

        void PlayDrawACard(Player player, Player target, int drawCardIdx) 
        {
            // assume drawCardIdx is in range
            var card = target.cards[drawCardIdx];
            target.cards.RemoveAt(drawCardIdx);

            player.cards.Add(card);
        }

        void Play99()
        {
            points = 99;
        }

        void PlayCurse(Player target)
        {
            target.alive = false;
        }

        void PlayRevive(Player target)
        {
            target.alive = true;
        }


        public List<Player> GetPlayers()
        {
            return players;
        }

        public int GetPoints()
        {
            return points;
        }

        public Player GetTurn()
        {
            return turn;
        }

        public string GetTurnId()
        {
            return turn.pid;
        }

        public bool GetGameOver()
        {
            return gameOver;
        }

        public Card GetLastDraw()
        {
            return lastDraw;
        }

        public string GetLastTurnId()
        {
            return lastTurn.pid;
        }

        public List<Card> GetUsed()
        {
            return used;
        }

        public List<Card> GetUnUsed()
        {
            return unused;
        }

        public Player GetWinner()
        {
            return winner;
        }

        public void SetDirection(bool direction)
        {
            this.direction = direction;
        }

        public bool GetDirection()
        {
            return direction;
        }

        public override string ToString()
        {
            string summary = $"---- GameState ----\n points: {points} direction: " +
                $"{direction} \n Turn: {turn.pid} \n used: {used.Count} unused: {unused.Count}\n" +
                $"turnIdx: {turnIdx} \n isGameOver: {gameOver} \n";
            string playerStats = "";

            foreach (var p in players)
            {
                string pstat = $"\nplayer {p.pid}\nhand: ";
                foreach (var c in p.cards)
                {
                    pstat += $"{c}, ";
                }
                pstat += "\n";

                playerStats += pstat;
            }

            return $"{summary}{playerStats}";
        }

    }


}


