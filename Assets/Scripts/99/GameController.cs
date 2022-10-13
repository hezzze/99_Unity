using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity;
using UnityEngine.UI;

namespace NinetyNine
{
    public class GameController : MonoBehaviour
    {
        public Text MessageText;
        public Text PointsValue;

        protected CardDealer cardDealer;

        //[SerializeField]
        //protected GameDataManager gameDataManager;

        protected GameState gameState;

        public List<Transform> PlayerPositions = new List<Transform>();
        //public List<Transform> BookPositions = new List<Transform>();

        [SerializeField]
        protected UIPlayer playerOne;
        [SerializeField]
        protected UIPlayer playerTwo;
        [SerializeField]
        protected UIPlayer playerThree;
        [SerializeField]
        protected UIPlayer playerFour;

        protected Dictionary<string, UIPlayer> uiPlayers = new Dictionary<string, UIPlayer>();
        protected string[] playerIds = new string[] { "offline-player", "bot1", "bot2", "bot3" };

        [SerializeField]
        protected UIPlayer currentTurnPlayer;
        [SerializeField]
        protected UIPlayer currentTurnTargetPlayer;

        [SerializeField]
        protected UICard selectedCard;

        [SerializeField]
        protected GameObject panel;

        [SerializeField]
        protected TargetSelector targetSelector;


        public enum UIState
        {
            Idle,
            GameStarted,
            TurnStarted,
            TurnSelectingNumber,
            TurnConfirmedSelectedNumber,
            TurnWaitingForOpponentConfirmation,
            TurnOpponentConfirmed,
            TurnGoFish,
            GameFinished
        };

        protected bool isDrawingCard = false;
        protected UICard cardToDraw = null;

        protected bool isSelectingPlusMinus = false;
        protected bool substractingSelected = false;

        protected bool isSelectingTarget = false;
        protected UIPlayer selectedTarget = null;


        [SerializeField]
        protected UIState uiState = UIState.Idle;



        protected void Awake()
        {
            //TODO refactor code, big time

            //Debug.Log("base awake");
            playerOne.PlayerId = playerIds[0];
            playerOne.PlayerName = "Player";
            //playerOne.Position = PlayerPositions[0].position;

            uiPlayers[playerOne.PlayerId] = playerOne;

            playerTwo.PlayerId = playerIds[1];
            playerTwo.PlayerName = "Bot1";
            //playerTwo.Position = PlayerPositions[1].position;
            playerTwo.IsAI = true;

            uiPlayers[playerTwo.PlayerId] = playerTwo;

            playerThree.PlayerId = playerIds[2];
            playerThree.PlayerName = "Bot2";
            //playerThree.Position = PlayerPositions[2].position;
            playerThree.IsAI = true;

            uiPlayers[playerThree.PlayerId] = playerThree;

            playerFour.PlayerId = playerIds[3];
            playerFour.PlayerName = "Bot3";
            //playerFour.Position = PlayerPositions[3].position;
            playerFour.IsAI = true;

            uiPlayers[playerFour.PlayerId] = playerFour;


            cardDealer = FindObjectOfType<CardDealer>();
            //panel = GameObject.FindWithTag("Panel");
            //panel.SetActive(false);

        }

        protected void Start()
        {
            StartCoroutine(InitAndStartGame());
            //GameFlow();
        }

        IEnumerator InitAndStartGame()
        {
            yield return new WaitForSeconds(1);

            // positions need to be set in co-routine, after the player have been
            // anchored responsively 
            playerOne.Position = PlayerPositions[0].position;
            playerTwo.Position = PlayerPositions[1].position;
            playerThree.Position = PlayerPositions[2].position;
            playerFour.Position = PlayerPositions[3].position;

            uiState = UIState.GameStarted;
            GameFlow();
        }


        //****************** Game Flow *********************//
        public virtual void GameFlow()
        {
            if (uiState > UIState.GameStarted)
            {
                //CheckPlayersBooks();
                ShowAndHidePlayersDisplayingCards();

                if (gameState.GetGameOver())
                {
                    uiState = UIState.GameFinished;
                }
            }

            switch (uiState)
            {
                case UIState.Idle:
                    {
                        Debug.Log("IDLE");
                        break;
                    }
                case UIState.GameStarted:
                    {
                        //Debug.Log("GameStarted");
                        OnGameStarted();
                        break;
                    }
                case UIState.TurnStarted:
                    {
                        Debug.Log("TurnStarted");
                        OnTurnStarted();
                        break;
                    }
                case UIState.TurnSelectingNumber:
                    {
                        //Debug.Log("TurnSelectingNumber");
                        OnTurnSelectingNumber();
                        break;
                    }
                case UIState.TurnConfirmedSelectedNumber:
                    {
                        Debug.Log("TurnComfirmedSelectedNumber");
                        OnTurnConfirmedSelectedNumber();
                        break;
                    }
                //case UIState.TurnWaitingForOpponentConfirmation:
                //    {
                //        Debug.Log("TurnWaitingForOpponentConfirmation");
                //        OnTurnWaitingForOpponentConfirmation();
                //        break;
                //    }
                //case UIState.TurnOpponentConfirmed:
                //    {
                //        Debug.Log("TurnOpponentConfirmed");
                //        OnTurnOpponentConfirmed();
                //        break;
                //    }
                //case UIState.TurnGoFish:
                //    {
                //        Debug.Log("TurnGoFish");
                //        OnTurnGoFish();
                //        break;
                //    }
                case UIState.GameFinished:
                    {
                        Debug.Log("GameFinished");
                        OnGameFinished();
                        break;
                    }
            }
        }

        protected virtual void OnGameStarted()
        {
            var values = GameState.buildDeckValues(Constants.N_CARD_IN_DECK);


            // TODO currently reuse the logic to set up the initial state
            // need to refactor this code to better honor the MVC practice
            cardDealer.InitializeUIDeck(values);
            cardDealer.DealDisplayingCards(playerOne, Constants.PLAYER_INITIAL_CARDS);
            cardDealer.DealDisplayingCards(playerTwo, Constants.PLAYER_INITIAL_CARDS, false);
            cardDealer.DealDisplayingCards(playerThree, Constants.PLAYER_INITIAL_CARDS, false);
            cardDealer.DealDisplayingCards(playerFour, Constants.PLAYER_INITIAL_CARDS, false);

            Debug.Log("Player cards dealt");


            var unused = new List<Card>();

            foreach (var uicard in cardDealer.uideck)
            {
                unused.Add(new Card(uicard.Rank, uicard.Suit));
            }

            var players = new List<Player>();

            foreach (var playerId in playerIds)
            {
                var player = new Player(playerId);

                var uihand = uiPlayers[playerId].uihand;

                foreach (var uicard in uihand)
                {
                    player.cards.Add(new Card(uicard.Rank, uicard.Suit));
                }

                players.Add(player);
            }

            gameState = new GameState(unused, players, 0);


            uiState = UIState.TurnStarted;
        }

        protected virtual void OnTurnStarted()
        {
            //set current turn player
            currentTurnPlayer = uiPlayers[gameState.GetTurnId()];

            Debug.Log(gameState);

            uiState = UIState.TurnSelectingNumber;
            GameFlow();
        }

        public void OnTurnSelectingNumber()
        {
            ResetSelectedCard();

            if (currentTurnPlayer == playerOne)
            {
                SetMessage($"Your turn. Pick a card from your hand.");
            }
            else
            {
                SetMessage($"{currentTurnPlayer.PlayerName}'s turn");
            }

            if (currentTurnPlayer.IsAI)
            {
                uiState = UIState.TurnConfirmedSelectedNumber;
                GameFlow();
            }
        }

        protected virtual void OnTurnConfirmedSelectedNumber()
        {

            if (currentTurnPlayer.IsAI)
            {
                // AI playing
                int idx = Random.Range(0, currentTurnPlayer.uihand.Count);
                selectedCard = currentTurnPlayer.uihand[idx];
                selectedCard.OnSelected(true);
                List<string> pids;

                switch (selectedCard.Rank)
                {
                    case Ranks.Jack:
                        // pick a random card to draw
                        pids = new List<string>();
                        foreach (var p in gameState.GetPlayers())
                        {
                            if (p.pid != currentTurnPlayer.PlayerId && p.alive) pids.Add(p.pid);
                        }
                        var randPid = pids[Random.Range(0, pids.Count)];
                        var randUiPlayer = uiPlayers[randPid];
                        cardToDraw = randUiPlayer.uihand[Random.Range(0, randUiPlayer.uihand.Count)];
                        break;
                    case Ranks.Ten:
                    case Ranks.Queen:
                        substractingSelected = Random.Range(0, 2) == 0;
                        break;
                    case Ranks.Ace:
                    case Ranks.Seven:
                        pids = new List<string>();
                        foreach (var p in gameState.GetPlayers())
                        {
                            if (p.pid != currentTurnPlayer.PlayerId && p.alive) pids.Add(p.pid);
                        }
                        selectedTarget = uiPlayers[pids[Random.Range(0, pids.Count)]];
                        break;
                }

                //if (selectedCard.Rank == Ranks.Jack)
                //{
                //    // pick a random card to draw
                //    List<string> pids = new List<string>();
                //    foreach (var id in playerIds)
                //    {
                //        if (id != currentTurnPlayer.PlayerId) pids.Add(id);
                //    }
                //    var randPid = pids[Random.Range(0, pids.Count)];
                //    var randUiPlayer = uiPlayers[randPid];
                //    cardToDraw = randUiPlayer.uihand[Random.Range(0, randUiPlayer.uihand.Count)];
                //}
            }


            SetMessage($"{currentTurnPlayer.PlayerName} played {selectedCard} ...");

            Debug.Log($"{currentTurnPlayer.PlayerName} played {selectedCard} ...");

            var cardToPlay = new Card(selectedCard.Rank, selectedCard.Suit);

            // UI to update gameState by callong makeMove


            switch (selectedCard.Rank)
            {
                case Ranks.Jack:
                    var targetUiPlayer = uiPlayers[cardToDraw.OwnerId];
                    var drawIdx = targetUiPlayer.uihand.IndexOf(cardToDraw);
                    gameState.makeMove(currentTurnPlayer.PlayerId, cardToPlay, targetUiPlayer.PlayerId, new How(null, drawIdx));
                    break;
                case Ranks.Ten:
                case Ranks.Queen:
                    gameState.makeMove(currentTurnPlayer.PlayerId, cardToPlay, null, new How(substractingSelected, null));
                    break;
                case Ranks.Ace:
                case Ranks.Seven:
                    gameState.makeMove(currentTurnPlayer.PlayerId, cardToPlay, selectedTarget.PlayerId, null);
                    break;
                default:
                    gameState.makeMove(currentTurnPlayer.PlayerId, cardToPlay, null, null);
                    break;
            }



            //if (selectedCard.Rank == Ranks.Jack)
            //{
            //    var targetUiPlayer = uiPlayers[cardToDraw.OwnerId];
            //    var drawIdx = targetUiPlayer.uihand.IndexOf(cardToDraw);
            //    gameState.makeMove(currentTurnPlayer.PlayerId, cardToPlay, targetUiPlayer.PlayerId, new How(null, drawIdx));
            //}
            //else
            //{
            //    gameState.makeMove(currentTurnPlayer.PlayerId, cardToPlay, null, null);
            //}
            Debug.Log(gameState);

            Card lastDraw = gameState.GetLastDraw();

            // for some card last draw could be null if no card is drawn
            if (lastDraw != null)
            {
                //Debug.Log($"Deal {lastDraw} to {currentTurnPlayer.PlayerId}...");
                cardDealer.DealCardToPlayer(currentTurnPlayer, lastDraw.Rank, lastDraw.Suit);
            }

            currentTurnPlayer.PlayCard(cardDealer, selectedCard);


            // UI updates for cards

            switch (selectedCard.Rank)
            {
                case Ranks.Jack:
                    uiPlayers[cardToDraw.OwnerId].SendACardToPlayer(currentTurnPlayer, cardDealer, cardToDraw, !currentTurnPlayer.IsAI);
                    break;
                case Ranks.Seven:
                    cardDealer.ExchangeCards(currentTurnPlayer, !currentTurnPlayer.IsAI, selectedTarget, !selectedTarget.IsAI);
                    break;
            }


            // reset

            selectedCard = null;
            cardToDraw = null;
            isDrawingCard = false;
            isSelectingPlusMinus = false;
            substractingSelected = false;
            isSelectingTarget = false;
            selectedTarget = null;

            // TODO refactor
            // After animation ended, a new trun will be started automatically


        }

        //public void OnTurnWaitingForOpponentConfirmation()
        //{
        //    if (currentTurnTargetPlayer.IsAI)
        //    {
        //        uiState = UIState.TurnOpponentConfirmed;
        //        GameFlow();
        //    }
        //}

        //protected virtual void OnTurnOpponentConfirmed()
        //{
        //    List<byte> cardValuesFromTargetPlayer = gameDataManager.TakeCardValuesWithRankFromPlayer(currentTurnTargetPlayer, selectedRank);

        //    if (cardValuesFromTargetPlayer.Count > 0)
        //    {
        //        gameDataManager.AddCardValuesToPlayer(currentTurnPlayer, cardValuesFromTargetPlayer);

        //        bool senderIsLocalPlayer = currentTurnTargetPlayer == localPlayer;
        //        currentTurnTargetPlayer.SendDisplayingCardToPlayer(currentTurnPlayer, cardDealer, cardValuesFromTargetPlayer, senderIsLocalPlayer);
        //        uiState = UIState.TurnSelectingNumber;
        //    }
        //    else
        //    {
        //        uiState = UIState.TurnGoFish;
        //        GameFlow();
        //    }
        //}

        //protected virtual void OnTurnGoFish()
        //{
        //    SetMessage($"Go fish!");

        //    byte cardValue = gameDataManager.DrawCardValue();

        //    if (cardValue == Constants.POOL_IS_EMPTY)
        //    {
        //        Debug.LogError("Pool is empty");
        //        return;
        //    }

        //    if (Card.GetRank(cardValue) == selectedRank)
        //    {
        //        cardDealer.DrawDisplayingCard(currentTurnPlayer, cardValue);
        //    }
        //    else
        //    {
        //        cardDealer.DrawDisplayingCard(currentTurnPlayer);
        //        uiState = UIState.TurnStarted;
        //    }

        //    gameDataManager.AddCardValueToPlayer(currentTurnPlayer, cardValue);
        //}

        public void OnGameFinished()
        {
            if (gameState.GetWinner().pid == playerOne.PlayerId)
            {
                SetMessage($"You WON!");
            }
            else
            {
                SetMessage($"You LOST!");
            }
        }

        ////****************** Helper Methods *********************//
        public void ResetSelectedCard()
        {
            if (selectedCard != null)
            {
                selectedCard.OnSelected(false);
                selectedCard = null;
            }
        }

        protected void SetMessage(string message)
        {
            MessageText.text = message;
        }

        protected void SetPoints(int point)
        {
            PointsValue.text = $"{point}";
        }

        //public void PlayerShowBooksIfNecessary(Player player)
        //{
        //    Dictionary<Ranks, List<byte>> books = gameDataManager.GetBooks(player);

        //    if (books != null)
        //    {
        //        foreach (var book in books)
        //        {
        //            player.ReceiveBook(book.Key, cardDealer);

        //            gameDataManager.RemoveCardValuesFromPlayer(player, book.Value);
        //            gameDataManager.AddBooksForPlayer(player, book.Key);
        //        }
        //    }
        //}

        ////public void CheckPlayersBooks()
        ////{
        ////    List<byte> playerCardValues = gameDataManager.PlayerCards(localPlayer);
        ////    localPlayer.SetCardValues(playerCardValues);
        ////    PlayerShowBooksIfNecessary(localPlayer);

        ////    playerCardValues = gameDataManager.PlayerCards(remotePlayer);
        ////    remotePlayer.SetCardValues(playerCardValues);
        ////    PlayerShowBooksIfNecessary(remotePlayer);
        ////}

        public void ShowAndHidePlayersDisplayingCards()
        {
            playerOne.ShowCardValues();

            playerTwo.HideCardValues();
            playerThree.HideCardValues();
            playerFour.HideCardValues();
        }

        List<UIPlayer> GetAliveUIPlayers()
        {
            List<UIPlayer> results = new List<UIPlayer>();

            var players = gameState.GetPlayers();

            foreach (var p in players)
            {
                var uiplayer = uiPlayers[p.pid];
                if (p.alive && uiplayer != currentTurnPlayer) results.Add(uiplayer);
            }

            return results;
        }

        ////****************** User Interaction *********************//
        public void OnCardSelected(UICard uicard)
        {
            if (uiState == UIState.TurnSelectingNumber && uicard.OwnerId != null)
            {
                //Debug.Log($"Owner Id {uicard.OwnerId}, pid: {currentTurnPlayer.PlayerId}");
                if (uicard.OwnerId == currentTurnPlayer.PlayerId)
                {

                    // 1. reset
                    if (selectedCard != null)
                    {
                        selectedCard.OnSelected(false);
                        isDrawingCard = false;
                        isSelectingPlusMinus = false;
                        substractingSelected = false;
                        isSelectingTarget = false;
                        selectedTarget = null;
                        panel.SetActive(false);
                        targetSelector.gameObject.SetActive(false);

                        if (cardToDraw != null)
                        {
                            cardToDraw.OnSelected(false);
                            cardToDraw = null;
                        }
                    }

                    // 2. set selected card 
                    selectedCard = uicard;
                    selectedCard.OnSelected(true);
                    SetMessage($"{currentTurnPlayer.PlayerName}: Play {selectedCard} ?");


                    // 3. show extra UI options
                    switch (uicard.Rank)
                    {
                        case Ranks.Jack:
                            isDrawingCard = true;
                            break;
                        case Ranks.Ten:
                        case Ranks.Queen:
                            isSelectingPlusMinus = true;
                            panel.SetActive(true);
                            break;
                        case Ranks.Ace:
                        case Ranks.Seven:
                            isSelectingTarget = true;
                            targetSelector.RenderButtons(GetAliveUIPlayers(), OnTargetSelected);
                            targetSelector.gameObject.SetActive(true);
                            break;
                    }


                }
                else if (isDrawingCard)
                {
                    if (cardToDraw != null)
                    {
                        cardToDraw.OnSelected(false);
                    }

                    cardToDraw = uicard;
                    uicard.OnSelected(true);
                    SetMessage($"Draw this card from {uicard.OwnerId} ?");
                }
            }
        }

        public virtual void OnOkSelected()
        {
            if (uiState == UIState.TurnSelectingNumber && playerOne == currentTurnPlayer)
            {
                if (selectedCard != null)
                {
                    if (selectedCard.Rank == Ranks.Jack && cardToDraw == null)
                    {
                        SetMessage($"Please select a card to draw");
                    }
                    else if (selectedCard.Rank == Ranks.Ace && selectedTarget == null)
                    {
                        SetMessage($"Please select a target...");
                    }
                    else
                    {
                        uiState = UIState.TurnConfirmedSelectedNumber;
                        GameFlow();
                    }
                }
            }
            //else if (uiState == UIState.TurnWaitingForOpponentConfirmation && playerOne == currentTurnTargetPlayer)
            //{
            //    uiState = UIState.TurnOpponentConfirmed;
            //    GameFlow();
            //}
        }

        public virtual void OnPlusMinusSelected()
        {
            if (uiState == UIState.TurnSelectingNumber && isSelectingPlusMinus)
            {
                if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name == Constants.MINUS_BUTTON_NAME)
                {
                    substractingSelected = true;
                }
                panel.SetActive(false);
                SetMessage($"Play {selectedCard} to " +
                    $"{(substractingSelected ? "substract" : "add")} " +
                    $"{(selectedCard.Rank == Ranks.Ten ? 10 : 20)} points?");
            }
        }

        void OnTargetSelected(string PlayerId, string PlayerName)
        {
            selectedTarget = uiPlayers[PlayerId];
            targetSelector.gameObject.SetActive(false);

            switch (selectedCard.Rank)
            {
                case Ranks.Ace:
                    SetMessage($"Play {selectedCard} to choose {PlayerName} to play " +
                    "next?");
                    break;
                case Ranks.Seven:
                    SetMessage($"Play {selectedCard} to exchange cards with {PlayerName}?");
                    break;
            }
        }

        //****************** Animator Event *********************//
        public virtual void AllAnimationsFinished()
        {
            if (uiState == UIState.TurnConfirmedSelectedNumber)
            {
                // reshuffle at the end of turn if needed
                // after reshuffling, the used card list is cleared
                if (gameState.GetUsed().Count == 0)
                {

                    Debug.Log("Reshuffle..");

                    var unused = gameState.GetUnUsed();
                    List<byte> values = new List<byte>();

                    foreach (var card in unused)
                    {
                        values.Add(card.value);
                    }

                    cardDealer.refillDeck(values);
                }

                // UI updates based on new state

                var players = gameState.GetPlayers();

                foreach (var p in players)
                {
                    if (!p.alive) uiPlayers[p.pid].ShowPanel();
                }

                SetPoints(gameState.GetPoints());


                if (gameState.GetGameOver())
                {
                    uiState = UIState.GameFinished;
                }
                else
                {
                    uiState = UIState.TurnStarted;
                }

            }

            GameFlow();
        }
    }
}
