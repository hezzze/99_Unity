using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NinetyNine
{
    public class Player
    {
        public bool alive = true;
        public List<Card> cards = new List<Card>();
        public string pid;

        public Player(string pid) {
            this.pid = pid;
        }

    }
}
