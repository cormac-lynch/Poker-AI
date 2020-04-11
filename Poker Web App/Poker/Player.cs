using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Poker_Web_App.Poker
{
    /// <summary>
    /// 
    /// </summary>
    public class Player
    {
        /// <summary>
        /// 
        /// </summary>
        public int chips;

        /// <summary>
        /// 
        /// </summary>
        public int moneyInPotThisRound;

        /// <summary>
        /// 
        /// </summary>
        public int totalMoneyInPot;

        /// <summary>
        /// 
        /// </summary>
        public Hand hand;

        /// <summary>
        /// 
        /// </summary>
        public bool isDealer;

        /// <summary>
        /// 
        /// </summary>
        public string name;

        /// <summary>
        /// 
        /// </summary>
        public Player(int chips, Hand hand, bool isDealer, string name, int moneyInPotThisRound)
        {
            this.chips = chips;
            this.hand = hand;
            this.isDealer = isDealer;
            this.name = name;
            this.moneyInPotThisRound = moneyInPotThisRound;
        }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public Player()
        {

        }
    }
}