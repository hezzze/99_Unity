using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.U2D;

namespace NinetyNine
{
    public class Card : IEquatable<Card>
    {
        public Suits Suit = Suits.NoSuits;
        public Ranks Rank = Ranks.NoRanks;

        public byte value
        {
            get
            {
                if (Rank == Ranks.BlackJoker) return 52;
                if (Rank == Ranks.RedJoker) return 53;
                return (byte)((int)Rank * 4 + (int)Suit);
            }
        }

        public static Ranks GetRank(byte value)
        {
            if (value == 52)
            {
                return Ranks.BlackJoker;
            } else if (value == 53)
            {
                return Ranks.RedJoker;
            } else
            {
                // 0-3 are 1's
                // 4-7 are 2's
                // ...
                // 48-51 are kings's
                return (Ranks)(value / 4);
            }
        }

        public static Suits GetSuit(byte value)
        {
            if (value > 51)
            {
                return Suits.NoSuits;
            } else
            {
                // 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48 are Spades(0)
                return (Suits)(value % 4);
            }
        }

        public Card(Ranks rank, Suits suit)
        {
            this.Rank = rank;
            if (rank == Ranks.BlackJoker || rank == Ranks.RedJoker)
            {
                this.Suit = Suits.NoSuits;
            } else
            {
                this.Suit = suit;
            }
        }

        public Card(byte value)
        {
            Rank = GetRank(value);
            Suit = GetSuit(value);
        }

        public bool Equals(Card other)
        {
            return Suit == other.Suit && Rank == other.Rank;
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }

        public string GetRankDescription()
        {
            FieldInfo fieldInfo = Rank.GetType().GetField(Rank.ToString());
            DescriptionAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes[0].Description;
        }

        public string GetSuitDescription()
        {
            FieldInfo fieldInfo = Suit.GetType().GetField(Suit.ToString());
            DescriptionAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes[0].Description;
        }

        public string GetCode()
        {
            return $"{GetSuitDescription()}{GetRankDescription()}";
        }
    }
}

