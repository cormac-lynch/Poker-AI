using System;
using System.Collections.Generic;
using System.Text;

namespace Poker_Web_App.Poker
{
    /// <summary>
    /// Class describing an action a player made during a hand
    /// </summary>
    public class Action
    {
        /// <summary>
        /// Describes the number of the action in the sequence of actions
        /// </summary>
        public int actionNo;

        /// <summary>
        /// Describes the type of action the player made
        /// 0 - Fold | 1 - Post SB | 2 Post BB | 23 - Raise | 3 - Call | 4 - Check | 5 - Bet | 7 - All-In
        /// </summary>
        public int actionType;

        /// <summary>
        /// Username of the player who made the action
        /// </summary>
        public string player;


        /// <summary>
        /// Sum of bet in blinds (value of the bet ÷ value of a big blind)
        /// </summary>
        public double sum;

        public Action(int actionNo, int actionType, string player, double sum)
        {
            this.actionNo = actionNo;
            this.actionType = actionType;
            this.player = player;
            this.sum = sum;
        }

        public Action()
        {

        }

        /// <summary>
        /// Method for comparing this action to another action to see if they are similar
        /// Two actions are similar if they have the same action type and bet size
        /// </summary>
        /// <param name="action"> Action to compare against </param>
        /// <returns> True or False </returns>
        public bool IsSimilarTo(Action action)
        {
            bool isSimilar = false;

            // 20% leeway
            double margin = this.sum * .20;

            // If action type is the same and sums are the same w/ margin. In the case of an all in, ignore the bet size
            if ((this.actionType == action.actionType) && (action.sum >= (this.sum - margin) && action.sum <= (this.sum + margin)) ||
                (this.actionType == action.actionType) && (this.actionType == 7))
            {
                isSimilar = true;
            }

            return isSimilar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Type: " + this.actionType + ", Sum: " + this.sum;
        }
    }
}
