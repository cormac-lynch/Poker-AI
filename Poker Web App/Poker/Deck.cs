using System;
using System.Collections.Generic;

namespace Poker_Web_App.Poker
{
    /// <summary>
    /// 
    /// </summary>
    public class Deck
    {
        /// <summary>
        /// 
        /// </summary>
        public List<Card> cards;

        /// <summary>
        /// 
        /// </summary>
        public Deck()
        {
            this.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Shuffle()
        {
            Random rand = new Random();

            List<Card> shuffledDeck = new List<Card>();

            for (int i = 52; i > 0; i--)
            {
                int cardIndex = rand.Next(0, i);

                shuffledDeck.Add(this.cards[cardIndex]);
                this.cards.RemoveAt(cardIndex);
            }

            this.cards = shuffledDeck;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            cards = new List<Card>();

            char[] suits = { 's', 'c', 'h', 'd' };

            foreach (char suit in suits)
            {
                for (int value = 2; value <= 14; value++)
                {
                    cards.Add(new Card(value, suit));
                }
            }
        }
    }
}