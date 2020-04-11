using System;
using System.Collections.Generic;
using System.Text;

namespace Poker_Web_App.Poker
{
    /// <summary>
    /// Class for storing information about a players hand
    /// </summary>
    public class Hand
    {
        /// <summary>
        /// List for storing a players two cards
        /// </summary>
        public List<Card> cards;

        /// <summary>
        /// Constructor for Hand
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        public Hand(Card c1, Card c2)
        {
            cards = new List<Card>();

            if(c1.value >= c2.value)
            {
                this.cards.Add(c1);
                this.cards.Add(c2);
            }
            else
            {
                this.cards.Add(c2);
                this.cards.Add(c1);
            }

        }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public Hand()
        {

        }

        /// <summary>
        /// Constructor for Hand
        /// </summary>
        /// <param name="handAsString"> String describing the hand e.g. "AhAc"</param>
        public Hand(string handAsString)
        {
            cards = new List<Card>();

            string[] cardsAsStrings = new string[2];
            cardsAsStrings[0] = handAsString.Substring(0, 2);
            cardsAsStrings[1] = handAsString.Substring(2, 2);

            this.cards.Add(new Card(cardsAsStrings[0]));
            this.cards.Add(new Card(cardsAsStrings[1]));
        }

        /// <summary>
        /// Method for checking if this hand is suited
        /// </summary>
        /// <returns> True if the hand is suited </returns>
        public bool IsSuited()
        {
            return this.cards[0].suit == this.cards[1].suit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        public static bool operator ==(Hand h1, Hand h2)
        {
            return h1.cards[0] == h2.cards[0] && h1.cards[1] == h2.cards[1] || h1.cards[0] == h2.cards[1] && h1.cards[1] == h2.cards[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool operator !=(Hand h1, Hand h2)
        {
            return (h1.cards[0] != h2.cards[0] || h1.cards[1] != h2.cards[1]) && (h1.cards[0] != h2.cards[1] && h1.cards[1] != h2.cards[0]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.cards[0].ToString() + this.cards[1].ToString();
        }

        /// <summary>
        /// Compares this hand with Hand h, they are similar if they have the same card values and if they are both suited/unsuited
        /// </summary>
        /// <param name="h"> Hand to compare against </param>
        /// <param name="checkSuit"> Bool value indicating if the suits of the cards should be taken into account</param>
        /// <returns> True if hands are similar </returns>
        public bool IsSimilarTo(Hand h, bool checkSuit)
        {
            // If the card values are the same
            bool isSimilar = (this.cards[0].value == h.cards[0].value && this.cards[1].value == h.cards[1].value) ||
                             (this.cards[0].value == h.cards[1].value && this.cards[1].value == h.cards[0].value);

            if (checkSuit)
            {
                // If both hands are suited or unsuited
                isSimilar = isSimilar && (this.IsSuited() == h.IsSuited());
            }

            return isSimilar;
        }

        public double GetDistanceTo(Hand h)
        {
            double distance = 0;

            // Create a copy of the hand so the actual object is not modified
            Hand tempHand = new Hand(new Card(this.cards[0].value, this.cards[0].suit), new Card(this.cards[1].value, this.cards[1].suit));

            // While temphand does not have the same card values as h
            while (!tempHand.IsSimilarTo(h, false))
            {
                // Try move up/down the grid if the two hands are not on the same row
                if (tempHand.cards[1].value != (h.cards[1].value - (h.cards[0].value - tempHand.cards[0].value)) && (tempHand.cards[1].value != 2 || tempHand.cards[1].value < (h.cards[1].value - (h.cards[0].value - tempHand.cards[0].value))))
                {
                    distance += 1;

                    // If hand is moving over +2.5 weighted grid line (the line between pocket pairs and regular hands)
                    if (tempHand.cards[0].value == tempHand.cards[1].value || (tempHand.cards[0].value == tempHand.cards[1].value + 1 && tempHand.cards[1].value < (h.cards[1].value - (h.cards[0].value - tempHand.cards[0].value))))
                    {
                        distance += 2.5;
                    }

                    // If temp hand needs to be moved down a row
                    if (tempHand.cards[1].value > (h.cards[1].value - (h.cards[0].value - tempHand.cards[0].value)))
                    {
                        tempHand.cards[1].value--;
                    }
                    else
                    {
                        tempHand.cards[1].value++;
                    }
                }
                // Move horizontally
                else
                {
                    // Moving horizontally has an added weight of .5
                    distance += 1.5;

                    // If tempHand needs to be moved back a column
                    if (tempHand.cards[0].value > h.cards[0].value)
                    {
                        tempHand.cards[0].value -= 1;
                        tempHand.cards[1].value -= 1;
                    }
                    else
                    {
                        tempHand.cards[0].value += 1;
                        tempHand.cards[1].value += 1;
                    }
                }
            }

            return distance;
        }

    }
}
