using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;


namespace NinetyNine
{
    public class AIPlayer : Agent
    {
        static string[] SINGLE_ACTION = new string[] { "2", "3", "5", "6", "8", "9", "4", "K" };
        static string[] DUAL_ACTION = new string[] { "T", "Q" };
        static string[] CHOOSE_ACTION = new string[] { "A", "7", "J" };
        static string[] SIGN = new string[] { "+", "-" };


        GameState st = null;
        Dictionary<string, byte> _actionSpace;
        string[] _actionList;

        Dictionary<string, Suits> _suitMap;
        Dictionary<string, Ranks> _rankMap;
        GameController gc;

        // Start is called before the first frame update
        protected void Awake()
        {
            _actionSpace = new Dictionary<string, byte>();
            byte index = 0;

            _suitMap = new Dictionary<string, Suits>
            {
                ["S"] = Suits.Spades,
                ["H"] = Suits.Hearts,
                ["D"] = Suits.Diamonds,
                ["C"] = Suits.Clubs
            };

            _rankMap = new Dictionary<string, Ranks>
            {
                ["A"] = Ranks.Ace,
                ["2"] = Ranks.Two,
                ["3"] = Ranks.Three,
                ["4"] = Ranks.Four,
                ["5"] = Ranks.Five,
                ["6"] = Ranks.Six,
                ["7"] = Ranks.Seven,
                ["8"] = Ranks.Eight,
                ["9"] = Ranks.Nine,
                ["T"] = Ranks.Ten,
                ["J"] = Ranks.Jack,
                ["Q"] = Ranks.Queen,
                ["K"] = Ranks.King
            };

            // static string[] SUITS = new string[] { "S", "H", "D", "C" };
            //
            // SINGLE_ACTION = ["2", "3", "5", "6", "8", "9", "4", "K"]
            // DUAL_ACTION = ["T", "Q"]
            // CHOOSE_ACTION = ["A", "7", "J"]
            //

            //for suit in SUITS:
            //    for action in SINGLE_ACTION:
            //        ACTION_SPACE[suit + action] = index
            //        index += 1
            //    for action in DUAL_ACTION:
            //        for sign in ["+", "-"]:
            //            ACTION_SPACE[suit + action + sign] = index
            //            index += 1
            //    for action in CHOOSE_ACTION:
            //        for idx in range(NUM_OF_PLAYERS):
            //            ACTION_SPACE[f"{suit}{action}->{idx}"] = index
            //            index += 1

            foreach (var suit in _suitMap.Keys)
            {
                foreach (var act in SINGLE_ACTION)
                {
                    _actionSpace[$"{suit}{act}"] = index;
                    index++;
                }
                foreach (var act in DUAL_ACTION)
                {
                    foreach (var sign in SIGN)
                    {
                        _actionSpace[$"{suit}{act}{sign}"] = index;
                        index++;
                    }
                }
                foreach (var act in CHOOSE_ACTION)
                {
                    for (byte i = 0; i < Constants.NUM_OF_PLAYERS; i++)
                    {
                        _actionSpace[$"{suit}{act}->{i}"] = index;
                        index++;
                    }
                }
            }

            foreach (var pair in _actionSpace)
            {
                Debug.Log($"{pair.Key}: {pair.Value}");
            }

            _actionList = new string[_actionSpace.Keys.Count];
            _actionSpace.Keys.CopyTo(_actionList, 0);

            var acts = "";

            foreach (var act in _actionList)
            {
                acts += $",{act}";
            }

            Debug.Log(acts);

        }

        public void GetInference(GameController controller)
        {
            gc = controller;
            st = controller.GetGameState();
            RequestDecision();
        }

        public override void CollectObservations(VectorSensor sensor)
        {

            Debug.Log("### AI Observing...");

            // based on the game state create the one-hot vector encoding of the state

            // https://github.com/Unity-Technologies/ml-agents/blob/release_19_docs/docs/Learning-Environment-Design-Agents.md#vector-observations

            // make observations based on the current player state
            var aiPlayer = st.GetTurn();

            var cardSet = new HashSet<byte>();

            for (int ci = 0; ci < aiPlayer.cards.Count; ci++)
            {
                cardSet.Add(aiPlayer.cards[ci].value);
            }

            // state is encoded as 52 1-hot vector 
            for (byte n = 0; n < Constants.N_CARD_IN_DECK; n++)
            {
                sensor.AddObservation(cardSet.Contains(n));
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            Debug.Log("### AI made an action...");

            // Get the action index for card pick
            int actionIndex = actionBuffers.DiscreteActions[0];

            var action = _actionList[actionIndex];
            var suit = _suitMap[$"{action[0]}"];
            var rank = _rankMap[$"{action[1]}"];

            bool isSub;
            int targetIdx;

            // P1: AI, other: random player

            switch (rank)
            {
                case Ranks.Jack:
                case Ranks.Ace:
                case Ranks.Seven:
                    // e.g. "SJ->1", "CJ->2"
                    targetIdx = int.Parse($"{action[4]}");
                    gc.OnAIAction(suit, rank, targetIdx, null);

                    //st.makeMove("P1", cardToPlay, players[targetIdx].pid, new How(null, drawIdx));
                    break;
                case Ranks.Ten:
                case Ranks.Queen:
                    // e.g. ST+, HQ- 
                    isSub = action[2] == '-';
                    gc.OnAIAction(suit, rank, null, isSub);
                    //st.makeMove("P1", cardToPlay, null, new How(isSub, null));
                    break;
                default:
                    gc.OnAIAction(suit, rank, null, null);
                    break;
            }


        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        // use this to define legal actions for the Agent
        // https://github.com/Unity-Technologies/ml-agents/blob/release_19_docs/docs/Learning-Environment-Design-Agents.md#masking-discrete-actions
        {
            //            full_actions = []
            //        hand = players[self.game_pointer].hand

            //        for card in hand:
            //            if card.rank in SINGLE_ACTION:
            //              full_actions.append(card.get_index())
            //            elif card.rank in DUAL_ACTION:
            //              # two actions each, plus/minus
            //              full_actions.extend([f"{card.get_index()}+", f"{card.get_index()}-"])
            //            elif card.rank in CHOOSE_ACTION:
            //              full_actions.extend(
            //                [
            //                    f"{card.get_index()}->{idx}"
            //                        for idx in range(self.num_players)
            //                        if players[idx].status == PlayerStatus.ALIVE
            //                        and idx != self.game_pointer
            //                    ]
            //                )

            Debug.Log("### AI calculating legal actions and mask them...");

            var legalAction = new HashSet<string>();
            var players = st.GetPlayers();
            var aiPlayer = st.GetTurn();

            foreach (var card in aiPlayer.cards)
            {
                if (SINGLE_ACTION.Contains(card.GetRankDescription()))
                {
                    legalAction.Add(card.GetCode());
                }
                else if (DUAL_ACTION.Contains(card.GetRankDescription()))
                {
                    legalAction.Add($"{card.GetCode()}+");
                    legalAction.Add($"{card.GetCode()}-");
                }
                else if (CHOOSE_ACTION.Contains(card.GetRankDescription()))
                {
                    for (var idx = 0; idx < players.Count; idx++)
                    {
                        // not choosing dead and self
                        if (players[idx].alive && players[idx].pid != aiPlayer.pid)
                        {
                            legalAction.Add($"{card.GetCode()}->{idx}");
                        }
                    }
                }

            }

            for (var i = 0; i < _actionList.Count(); i++)
            {
                // only 1 branch, which is set to 96 discrete actions
                actionMask.SetActionEnabled(0, i, legalAction.Contains(_actionList[i]));
            }

        }
    }

}
