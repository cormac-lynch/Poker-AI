using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poker_Web_App.Poker;

namespace Unit_Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        // Tests to see if the system evaluates the correct winner of a hand
        public void EvaluateWinnerOfHand1()
        {
            // Will have a straight
            Hand winHand = new Hand(new Card(14, 'c'), new Card(12, 's'));

            // Will have a two pair
            Hand loseHand = new Hand(new Card(4, 'd'), new Card(2, 's'));

            List<Card> tableCards = new List<Card>();
            tableCards.Add(new Card(11, 'h'));
            tableCards.Add(new Card(4, 'c'));
            tableCards.Add(new Card(10, 'd'));
            tableCards.Add(new Card(13, 's'));
            tableCards.Add(new Card(2, 'd'));

            PokerLogic p = new PokerLogic();

            // Evaluate Strengths
            HandStrength winStrength = p.GetHandStrength(winHand, tableCards);
            HandStrength loseStrength = p.GetHandStrength(loseHand, tableCards);

            int winner = p.GetShowdownWinner(winHand, loseHand, tableCards);

            Assert.AreEqual(winner,1);
        }

        [TestMethod]
        // Tests to see if the system evaluates the correct winner of a hand
        public void EvaluateWinnerOfHand2()
        {
            // Will have a two pair
            Hand winHand = new Hand(new Card(5, 'c'), new Card(5, 's'));

            // Will have a single pair
            Hand loseHand = new Hand(new Card(13, 's'), new Card(6, 's'));

            List<Card> tableCards = new List<Card>();
            tableCards.Add(new Card(11, 'c'));
            tableCards.Add(new Card(4, 'c'));
            tableCards.Add(new Card(3, 's'));
            tableCards.Add(new Card(12, 's'));
            tableCards.Add(new Card(3, 'c'));

            PokerLogic p = new PokerLogic();

            // Evaluate Strengths
            HandStrength winStrength = p.GetHandStrength(winHand, tableCards);
            HandStrength loseStrength = p.GetHandStrength(loseHand, tableCards);

            int winner = p.GetShowdownWinner(winHand, loseHand, tableCards);

            Assert.AreEqual(winner, 1);
        }

        [TestMethod]
        // Tests to see if the system evaluates the correct winner of a hand
        public void EvaluateWinnerOfHand3()
        {
            // Will have a flush
            Hand winHand = new Hand(new Card(14, 'c'), new Card(12, 'c'));

            // Will have a three of a kind
            Hand loseHand = new Hand(new Card(12, 'd'), new Card(12, 's'));

            List<Card> tableCards = new List<Card>();
            tableCards.Add(new Card(2, 'h'));
            tableCards.Add(new Card(4, 'c'));
            tableCards.Add(new Card(3, 'c'));
            tableCards.Add(new Card(11, 'c'));
            tableCards.Add(new Card(12, 'd'));

            PokerLogic p = new PokerLogic();

            // Evaluate Strengths
            HandStrength winStrength = p.GetHandStrength(winHand, tableCards);
            HandStrength loseStrength = p.GetHandStrength(loseHand, tableCards);

            int winner = p.GetShowdownWinner(winHand, loseHand, tableCards);

            Assert.AreEqual(winner, 1);
        }

        [TestMethod]
        // Tests to see if the system evaluates the correct winner of a hand
        public void EvaluateStrengthOfHand1()
        {
            // Will have a straight
            Hand hand = new Hand(new Card(11, 'c'), new Card(12, 's'));

            List<Card> tableCards = new List<Card>();
            tableCards.Add(new Card(14, 'h'));
            tableCards.Add(new Card(4, 'c'));
            tableCards.Add(new Card(13, 'h'));
            tableCards.Add(new Card(10, 's'));
            tableCards.Add(new Card(2, 'd'));

            PokerLogic p = new PokerLogic();

            // Evaluate Strength
            HandStrength handStrength = p.GetHandStrength(hand, tableCards);

            Assert.AreEqual(handStrength.strength, 5);
        }

        [TestMethod]
        // Tests to see if the system evaluates the correct winner of a hand
        public void EvaluateStrengthOfHand2()
        {
            // Will have a full house
            Hand hand = new Hand(new Card(8, 'c'), new Card(7, 's'));

            List<Card> tableCards = new List<Card>();
            tableCards.Add(new Card(8, 'h'));
            tableCards.Add(new Card(4, 'c'));
            tableCards.Add(new Card(8, 'h'));
            tableCards.Add(new Card(7, 's'));
            tableCards.Add(new Card(2, 'd'));

            PokerLogic p = new PokerLogic();

            // Evaluate Strength
            HandStrength handStrength = p.GetHandStrength(hand, tableCards);

            Assert.AreEqual(handStrength.strength, 3);
        }

        [TestMethod]
        // Tests to see if the system evaluates the correct distacne between two hands
        public void EvaluateHandDistance1()
        {
            Hand hand1 = new Hand(new Card(6, 'c'), new Card(6, 's'));
            Hand hand2 = new Hand(new Card(7, 'c'), new Card(4, 'd'));

            // The distacne should be 7
            Assert.AreEqual(hand1.GetDistanceTo(hand2), 7);
        }

        [TestMethod]
        // Tests to see if the system evaluates the correct distacne between two hands
        public void EvaluateHandDistance2()
        {
            Hand hand1 = new Hand(new Card(10, 'c'), new Card(7, 's'));
            Hand hand2 = new Hand(new Card(10, 'c'), new Card(5, 'd'));

            // The distacne should be 7
            Assert.AreEqual(hand1.GetDistanceTo(hand2), 2);
        }

        [TestMethod]
        // Tests to see if the system evaluates the correct distacne between two hands
        public void EvaluateHandDistance3()
        {
            Hand hand1 = new Hand(new Card(10, 'c'), new Card(7, 's'));
            Hand hand2 = new Hand(new Card(12, 'c'), new Card(9, 'd'));

            // The distacne should be 7
            Assert.AreEqual(hand1.GetDistanceTo(hand2), 3);
        }

    }
}
