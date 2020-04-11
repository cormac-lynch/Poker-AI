namespace Poker_Web_App.Poker
{
    /// <summary>
    /// Class for storing information about the strength of a hand with the table cards
    /// </summary>
    public class HandStrength
    {
        public int strength;

        public Card[] significantCards;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strength"></param>
        /// <param name="significantCards"></param>
        public HandStrength(int strength, Card[] significantCards)
        {
            this.strength = strength;
            this.significantCards = significantCards;
        }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public HandStrength()
        {
        }

    }
}
