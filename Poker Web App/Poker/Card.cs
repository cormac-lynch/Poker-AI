using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// 
/// </summary>
namespace Poker_Web_App.Poker
{
    /// <summary>
    /// Class for storing information about a given card
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Integer representing the value of the card
        /// </summary>
        public int value;

        /// <summary>
        /// Character representing the suit of the card
        /// </summary>
        public char suit;

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="value"> Integer representing the value of the card </param>
        /// <param name="suit"> Character representing the suit of the card </param>
        public Card(int value, char suit)
        {
            this.value = value;
            this.suit = suit;
        }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public Card()
        {

        }

        /// <summary>
        /// Converts string in form: suit in uppercase, value in single character uppercase
        /// e.g. DJ - jack of diamonds, C10 - ten of clubs, S9 - nine of spades
        /// </summary>
        public Card(string cardAsString)
        {
            if (cardAsString.Length == 2 || cardAsString.Length == 3)
            {
                // Check if the suit is valid
                if (cardAsString[0] == 'S' || cardAsString[0] == 'D' || cardAsString[0] == 'C' || cardAsString[0] == 'H')
                {
                    // Check to see if Card value is valid
                    if (Regex.IsMatch(cardAsString.Substring(1), @"^[2-9AKQJ]$|^10$"))
                    {
                        // Convert to lowercase before setting the suit
                        this.suit = cardAsString.Substring(0).ToLower()[0];

                        switch (cardAsString.Substring(1))
                        {
                            case "A":
                                this.value = 14;
                                break;
                            case "K":
                                this.value = 13;
                                break;
                            case "Q":
                                this.value = 12;
                                break;
                            case "J":
                                this.value = 11;
                                break;
                            case "10":
                                this.value = 10;
                                break;
                            default:
                                this.value = cardAsString[1] - '0';
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts the card to a string representing it's value and suit
        /// </summary>
        /// <returns> string representing card's value and suit </returns>
        public override string ToString()
        {
            StringBuilder card = new StringBuilder();

            switch(this.value)
            {
                case 10:
                    card.Append("T");
                    break;
                case 11:
                    card.Append("J");
                    break;
                case 12:
                    card.Append("Q");
                    break;
                case 13:
                    card.Append("K");
                    break;
                case 14:
                    card.Append("A");
                    break;
                default:
                    card.Append(this.value);
                    break;
            }

            card.Append(this.suit);

            return card.ToString();
        }

        /// <summary>
        /// Override for == operator
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns> bool value indicating if the two cards are equal </returns>
        public static bool operator ==(Card c1, Card c2)
        {
            return c1.value == c2.value && c1.suit == c2.suit;
        }

        /// <summary>
        /// Override for != operator
        /// </summary>
        /// <param name="c1"> First card to compare </param>
        /// <param name="c2"> Second card to compare </param>
        /// <returns> bool value indicating if the two cards are not equal </returns>
        public static bool operator !=(Card c1, Card c2)
        {
            return c1.value != c2.value || c1.suit != c2.suit;
        }

        /// <summary>
        /// Overide for Equals()
        /// </summary>
        /// <param name="obj"> Object to compare to</param>
        /// <returns> Bool value indicating if the object is equal to this</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Override for GetHashCode()
        /// </summary>
        /// <returns> Base hashcode </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
