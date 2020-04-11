using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker_Web_App.Poker
{
    /// <summary>
    /// Class for calculating poker game logic
    /// </summary>
    public class PokerLogic
    {
        public PokerLogic()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentHand"></param>
        /// <param name="tableCards"></param>
        /// <returns></returns>
        public HandStrength GetHandStrength(Hand currentHand, List<Card> tableCards)
        {
            // Create and fill the cards array with player hand and table cards
            Card[] cards = new Card[currentHand.cards.Count + tableCards.Count];
            Array.Copy(currentHand.cards.ToArray(), 0, cards, 0, 2);
            Array.Copy(tableCards.ToArray(), 0, cards, 2, tableCards.Count);

            HandStrength handStrength = new HandStrength();

            Card[] significantCards;

            significantCards = this.IsStraightFlush(cards);

            // #1 Royal Flush
            if (significantCards.Length > 0 && significantCards[0].value == 14)
            {
                handStrength.significantCards = significantCards;
                handStrength.strength = 0;
            }
            else
            {
                // #2 Straight Flush
                if (significantCards.Length > 0)
                {
                    handStrength.significantCards = significantCards;
                    handStrength.strength = 1;
                }
                else
                {
                    significantCards = this.IsFourOfAKind(cards);

                    // #3 Four of a Kind
                    if (significantCards.Length > 0)
                    {
                        handStrength.significantCards = significantCards;
                        handStrength.strength = 2;
                    }
                    else
                    {
                        significantCards = this.IsFullHouse(cards);

                        // #4 Full House
                        if (significantCards.Length > 0)
                        {
                            handStrength.significantCards = significantCards;
                            handStrength.strength = 3;
                        }
                        else
                        {
                            significantCards = this.IsFlush(cards, true);

                            // #5 Flush
                            if (significantCards.Length > 0)
                            {
                                handStrength.significantCards = significantCards;
                                handStrength.strength = 4;
                            }
                            else
                            {
                                significantCards = this.IsStraight(cards);

                                // #6 Straight
                                if (significantCards.Length > 0)
                                {
                                    handStrength.significantCards = significantCards;
                                    handStrength.strength = 5;
                                }
                                else
                                {
                                    significantCards = this.IsThreeOfAKind(cards);

                                    // #7 Three of a Kind
                                    if (significantCards.Length > 0) 
                                    {
                                        handStrength.significantCards = significantCards;
                                        handStrength.strength = 6;
                                    }
                                    else
                                    {
                                        significantCards = this.IsTwoPair(cards);

                                        // #8 Two Pair
                                        if (significantCards.Length > 0) 
                                        {
                                            handStrength.significantCards = significantCards;
                                            handStrength.strength = 7;
                                        }
                                        else
                                        {
                                            significantCards = this.IsPair(cards);

                                            // #9 One Pair
                                            if (significantCards.Length > 0)
                                            {
                                                handStrength.significantCards = significantCards;
                                                handStrength.strength = 8;
                                            }
                                            // #10 High Card
                                            else
                                            {
                                                significantCards = this.SortCards(cards);
                                                handStrength.significantCards = new Card[5];

                                                Array.Copy(significantCards, 0, handStrength.significantCards, 0, cards.Length < 5 ? cards.Length : 5);
                                                handStrength.strength = 9;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return handStrength;
        }

        /// <summary>
        /// Returns 1 if the user wins, -1 if AI wins, 0 for a tie
        /// </summary>
        /// <returns></returns>
        public int GetShowdownWinner(Hand hand1, Hand hand2, List<Card> tableCards)
        {
            int result = 0;

            PokerLogic p = new PokerLogic();

            HandStrength userStrength = p.GetHandStrength(hand1, tableCards);
            HandStrength aiStrength = p.GetHandStrength(hand2, tableCards);

            if (userStrength.strength < aiStrength.strength)
            {
                result = 1;
            }
            else if (userStrength.strength > aiStrength.strength)
            {
                result = -1;
            }
            else
            {
                // Check card by card to see who has the better hand
                for (int i = 0; i < userStrength.significantCards.Length; i++)
                {
                    if (userStrength.significantCards[i].value > aiStrength.significantCards[i].value)
                    {
                        result = 1;
                        break;
                    }
                    else if (userStrength.significantCards[i].value < aiStrength.significantCards[i].value)
                    {
                        result = -1;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public Card[] IsStraightFlush(Card[] cards)
        {
            Card[] significantCards = new Card[0];

            char flushSuit = this.GetFlushSuit(cards);

            if(flushSuit != 'n')
            {
                List<Card> tempSignificantCards = this.SortCards(cards).ToList();

                // Remove all non flush cards
                tempSignificantCards.RemoveAll(c => c.suit != flushSuit);

                significantCards = this.IsStraight(tempSignificantCards.ToArray());
            }

            return significantCards;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public Card[] IsFullHouse(Card[] cards)
        {
            // Create empty array for significant cards
            Card[] significantCards = new Card[0];

            if (cards.Length >= 5)
            {
                // Check for a three of a kind
                Card[] tempSignificantCards = this.IsThreeOfAKind(cards);

                if (tempSignificantCards.Length > 0)
                {
                    significantCards = new Card[5];

                    significantCards[0] = tempSignificantCards[0];
                    significantCards[1] = tempSignificantCards[1];
                    significantCards[2] = tempSignificantCards[2];

                    List<Card> tempCards = cards.ToList();

                    // Remove three of a kind from cards
                    tempCards.RemoveAll(c => c.value == tempSignificantCards[0].value);

                    // Check for pair
                    tempSignificantCards = this.IsPair(tempCards.ToArray());
                    tempSignificantCards = tempSignificantCards.Length > 0 ? tempSignificantCards : this.IsThreeOfAKind(tempCards.ToArray());

                    if (tempSignificantCards.Length > 0)
                    {
                        significantCards[3] = tempSignificantCards[0];
                        significantCards[4] = tempSignificantCards[1];
                    }
                    else
                    {
                        // Reset to empty array to indicate false
                        significantCards = new Card[0];
                    }
                }
            }

            return significantCards;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="sortCards"> Bool value indicating if cards should be sorted (if checking for straight flush etc dont sort)</param>
        /// <returns></returns>
        public Card[] IsFlush(Card[] cards, bool sortCards)
        {
            Card[] significantCards = new Card[0];

            char flushSuit = this.GetFlushSuit(cards);            

            if (flushSuit != 'n')
            {
                significantCards = new Card[5];

                if (sortCards)
                {
                    cards = this.SortCards(cards);
                }

                List<Card> tempCards = cards.ToList();
                foreach (Card c in cards)
                {
                    if (c.suit != flushSuit)
                    {
                        tempCards.Remove(c);
                    }
                }

                Array.Copy(tempCards.ToArray(), tempCards.Count - 5, significantCards, 0, 5);
            }

            return significantCards;
        }

        /// <summary>
        /// Checks if a given hand is a flush
        /// </summary>
        /// <param name="hand"> Player hand </param>
        /// <param name="tableCards"> Table cards </param>
        /// <returns> Bool indicating if it is a flush</returns>
        public Card[] IsStraight(Card[] cards)
        {
            Card[] significantCards = new Card[0];

            cards = this.RemoveDuplicates(cards);
            cards = this.SortCards(cards);

            // If there is an Ace
            if (cards[0].value == 14)
            {
                // Append ace to start
                Card[] temp = new Card[cards.Length + 1];
                temp[temp.Length - 1] = cards[0];
                Array.Copy(cards, 0, temp, 0, cards.Length);
                cards = temp;
            }

            int consecutiveCards = 0;
            int lastStraightIndex = -1;

            // Check for consecutive cards
            for (int i = 0; i <= cards.Length - 2; i++)
            {
                if (cards[i].value == cards[i + 1].value + 1 || (cards[i + 1].value == 14 && cards[i].value == 2))
                {
                    consecutiveCards++;
                }
                else
                {
                    consecutiveCards = 0;
                }

                if (consecutiveCards >= 4)
                {
                    lastStraightIndex = i + 1;
                    break;
                }
            }

            if (lastStraightIndex != -1)
            {
                significantCards = new Card[5];

                Array.Copy(cards, lastStraightIndex - 4, significantCards, 0, 5);
            }

            return significantCards;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="tableCards"></param>
        /// <returns></returns>
        public Card[] IsFourOfAKind(Card[] cards)
        {
            // Create empty array for significant cards
            Card[] significantCards = new Card[0];

            // Array for storing the number of occurences of each card
            int[] cardCounts = new int[13];

            foreach (Card c in cards)
            {
                // Increment the array by using each value as an index (minus two as the card values are 2-indexed)
                cardCounts[c.value - 2]++;
            }

            // int to store the value of the four of a kind's cards if one occurs
            int fourOfAKindValue = 0;

            for (int i = 0; i < cardCounts.Length; i++)
            {
                if (cardCounts[i] == 4)
                {
                    // Add two as the card values are 2-indexed
                    fourOfAKindValue = i + 2;
                }
            }

            // If there is a four of a kind
            if (fourOfAKindValue != 0)
            {
                significantCards = new Card[5];

                int significantIndex = 0;

                for (int i = 0; i < cards.Length; i++)
                {
                    if (cards[i].value == fourOfAKindValue)
                    {
                        significantCards[significantIndex] = cards[i];
                        significantIndex++;

                        // Remove a value from the array (inefficient, possibly change later)
                        List<Card> tempCards = cards.ToList();
                        tempCards.Remove(cards[i]);
                        cards = tempCards.ToArray();
                        i--;
                    }
                }

                cards = this.SortCards(cards);

                Array.Copy(cards, 0, significantCards, 4, cards.Length > 1 ? 1 : cards.Length);
            }

            return significantCards;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public Card[] IsThreeOfAKind(Card[] cards)
        {
            // Create empty array for significant cards
            Card[] significantCards = new Card[0];

            // Array for storing the number of occurences of each card
            int[] cardCounts = new int[13];

            foreach (Card c in cards)
            {
                // Increment the array by using each value as an index (minus two as the card values are 2-indexed)
                cardCounts[c.value - 2]++;
            }

            // int to store the value of the four of a kind's cards if one occurs
            int threeOfAKindValue = 0;

            for (int i = 0; i < cardCounts.Length; i++)
            {
                // If there is a three of a kind
                if (cardCounts[i] == 3)
                {
                    // Add two as the card values are 2-indexed
                    threeOfAKindValue = i + 2;
                }
            }

            // If there is a three of a kind
            if (threeOfAKindValue != 0)
            {
                significantCards = new Card[5];

                int significantIndex = 0;

                for (int i = 0; i < cards.Length; i++)
                {
                    if (cards[i].value == threeOfAKindValue)
                    {
                        significantCards[significantIndex] = cards[i];
                        significantIndex++;

                        // Remove a value from the array (inefficient, possibly change later)
                        List<Card> tempCards = cards.ToList();
                        tempCards.Remove(cards[i]);
                        cards = tempCards.ToArray();
                        i--;
                    }
                }

                cards = this.SortCards(cards);

                // Fill the rest of significant cards with the highest cards remaining
                Array.Copy(cards, 0, significantCards, 3, cards.Length > 2 ? 2 : cards.Length);
            }

            return significantCards;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public Card[] IsTwoPair(Card[] cards)
        {
            // Create empty array for significant cards
            Card[] significantCards = new Card[0];

            // Check for a pair
            Card[] tempSignificantCards = this.IsPair(cards);

            if(tempSignificantCards.Length > 0)
            {
                significantCards = new Card[5];

                significantCards[0] = tempSignificantCards[0];
                significantCards[1] = tempSignificantCards[1];

                List<Card> tempCards = cards.ToList();

                // Remove pair from cards
                tempCards.RemoveAll(c => c.value == tempSignificantCards[0].value);

                // Check for another pair
                tempSignificantCards = this.IsPair(tempCards.ToArray());

                if (tempSignificantCards.Length > 0)
                {
                    significantCards[2] = tempSignificantCards[0];
                    significantCards[3] = tempSignificantCards[1];

                    // Remove pair from cards
                    tempCards.RemoveAll(c => c.value == tempSignificantCards[0].value);

                    tempCards = this.SortCards(tempCards.ToArray()).ToList();

                    significantCards[4] = tempCards.First();
                }
                else
                {
                    // Reset to empty array to indicate false
                    significantCards = new Card[0];
                }
            }

            return significantCards;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public Card[] IsPair(Card[] cards)
        {
            // Create empty array for significant cards
            Card[] significantCards = new Card[0];

            if (cards.Length >= 2)
            {
                // Array for storing the number of occurences of each card
                int[] cardCounts = new int[13];

                foreach (Card c in cards)
                {
                    // Increment the array by using each value as an index (minus two as the card values are 2-indexed)
                    cardCounts[c.value - 2]++;
                }

                // int to store the value of the four of a kind's cards if one occurs
                int pairValue = 0;

                for (int i = 0; i < cardCounts.Length; i++)
                {
                    // If there is a three of a kind
                    if (cardCounts[i] == 2)
                    {
                        // Add two as the card values are 2-indexed
                        pairValue = i + 2;
                    }
                }

                // If there is a three of a kind
                if (pairValue != 0)
                {
                    significantCards = new Card[5];

                    int significantIndex = 0;

                    for (int i = 0; i < cards.Length; i++)
                    {
                        if (cards[i].value == pairValue)
                        {
                            significantCards[significantIndex] = cards[i];
                            significantIndex++;

                            // Remove a value from the array (inefficient, possibly change later)
                            List<Card> tempCards = cards.ToList();
                            tempCards.Remove(cards[i]);
                            cards = tempCards.ToArray();
                            i--;
                        }
                    }

                    cards = this.SortCards(cards);

                    // Fill the rest of significant cards with the highest cards remaining
                    Array.Copy(cards, 0, significantCards, 2, cards.Length > 3 ? 3 : cards.Length);
                }
            }

            return significantCards;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public Card[] RemoveDuplicates(Card[] cards)
        {
            // Remove duplicates
            for (int i = 0; i < cards.Length; i++)
            {
                for (int j = i + 1; j < cards.Length - 1; j++)
                {
                    if (cards[i].value == cards[j].value)
                    {
                        // Remove a value from the array (inefficient, possibly change later)
                        List<Card> tempCards = cards.ToList();
                        tempCards.Remove(cards[j]);
                        cards = tempCards.ToArray();
                    }
                }
            }

            return cards;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public Card[] SortCards(Card[] cards)
        {
            // Sort cards
            for (int i = 0; i < cards.Length; i++)
            {
                for (int j = i + 1; j < cards.Length; j++)
                {
                    if (cards[i].value <= cards[j].value)
                    {
                        // Swap Values
                        Card temp = cards[j];
                        cards[j] = cards[i];
                        cards[i] = temp;
                    }
                }
            }

            return cards;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public char GetFlushSuit(Card[] cards)
        {
            int heartCount = 0;
            int spadeCount = 0;
            int diamondCount = 0;
            int clubCount = 0;

            char flushSuit = 'n';

            foreach (Card c in cards)
            {
                switch (c.suit)
                {
                    case 's':
                        spadeCount++;
                        flushSuit = spadeCount == 5 ? 's' : flushSuit;
                        break;
                    case 'c':
                        clubCount++;
                        flushSuit = clubCount == 5 ? 'c' : flushSuit;
                        break;
                    case 'd':
                        diamondCount++;
                        flushSuit = diamondCount == 5 ? 'd' : flushSuit;
                        break;
                    case 'h':
                        heartCount++;
                        flushSuit = heartCount == 5 ? 'h' : flushSuit;
                        break;
                }
            }

            return flushSuit;
        }
    }
}
       