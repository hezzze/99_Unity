using System.ComponentModel;
using UnityEngine;

namespace NinetyNine
{
    public static class Constants
    {
        public const float PLAYER_CARD_POSITION_OFFSET = 1.2f;
        public const float PLAYER_BOOK_POSITION_OFFSET = 2f;
        public const float DECK_CARD_POSITION_OFFSET = 0.2f;
        public const string CARD_BACK_SPRITE = "cardBack_red5";
        public const float CARD_SELECTED_OFFSET = 0.3f;
        
        public const float CARD_MOVEMENT_SPEED = 25.0f;
        public const float CARD_SNAP_DISTANCE = 0.01f;
        public const float CARD_ROTATION_SPEED = 8f;
        public const float BOOK_MAX_RANDOM_ROTATION = 15f;
        public const byte POOL_IS_EMPTY = 255;

        public const byte N_CARD_IN_DECK = 52;
        public const int PLAYER_INITIAL_CARDS = 5;
        public const float PLAY_CARD_OFFSET = 3f;

        public const string PLUS_BUTTON_NAME = "plus";
        public const string MINUS_BUTTON_NAME = "minus";
        public const byte NUM_OF_PLAYERS = 4;
    }

    public enum Suits
    {
        [Description("No Suits")]
        NoSuits = -1,
        [Description("S")]
        Spades = 0,
        [Description("H")]
        Hearts = 1,
        [Description("D")]
        Diamonds = 2,
        [Description("C")]
        Clubs = 3,
    }

    public enum Ranks
    {
        [Description("No Ranks")]
        NoRanks = -1,
        [Description("A")]
        Ace = 0,
        [Description("2")]
        Two = 1,
        [Description("3")]
        Three = 2,
        [Description("4")]
        Four = 3,
        [Description("5")]
        Five = 4,
        [Description("6")]
        Six = 5,
        [Description("7")]
        Seven = 6,
        [Description("8")]
        Eight = 7,
        [Description("9")]
        Nine = 8,
        [Description("T")]
        Ten = 9,
        [Description("J")]
        Jack = 10,
        [Description("Q")]
        Queen = 11,
        [Description("K")]
        King = 12,
        [Description("Black Joker")]
        BlackJoker = 13,
        [Description("Red Joker")]
        RedJoker = 14
    }
}
