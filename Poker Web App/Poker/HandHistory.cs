using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Poker_Web_App.Poker
{
    /// <summary>
    /// Class for storing a single hand from a hand history
    /// </summary>
    public class HandHistory
    {
        /// <summary>
        /// Hero's hand (hand of player who's perspective we are taking) 
        /// </summary>
        public Hand hand;

        /// <summary>
        /// Object representing the strength of a players hand e.g. pair, two pair, etc.
        /// </summary>
        public HandStrength handStrength;

        /// <summary>
        /// Bool value indicating if hero is the dealer
        /// </summary>
        public bool isDealer;

        /// <summary>
        /// List of table cards
        /// </summary>
        public List<Card> tableCards;

        /// <summary>
        /// List of actions for each round (blinds, preflop, postflop, turn, river)
        /// </summary>
        public List<List<Action>> rounds;

        /// <summary>
        /// int value indicating what round the game is currently in, -1 if the hand is finished
        /// </summary>
        public int currentRound;    

        /// <summary>
        /// Ration of chips the hero has compared to the villain
        /// </summary>
        public double heroBlindRatio;

        /// <summary>
        /// Creates empty HandHistory object
        /// </summary>
        public HandHistory()
        {

        }

        public HandHistory(BsonDocument document)
        {
            this.hand = new Hand(new Card(document["hand"][0]["value"].ToInt32(), (char)int.Parse(document["hand"][0]["suit"].ToString())), new Card(document["hand"][1]["value"].ToInt32(), (char)int.Parse(document["hand"][1]["suit"].ToString())));

            this.handStrength = new HandStrength();
            this.handStrength.strength = document["handStrength"]["strength"].ToInt32();

            this.handStrength.significantCards = new Card[5];

            this.tableCards = new List<Card>();

            for (int i = 0; i < document["tableCards"].AsBsonArray.Count(); i++)
            {
                this.tableCards.Add(
                    new Card(document["tableCards"][i]["value"].ToInt32(), (char)int.Parse(document["tableCards"][i]["suit"].ToString()))
                );
            }

            this.isDealer = document["isDealer"].ToBoolean();

            this.heroBlindRatio = document["heroBlindRatio"].ToDouble();

            this.currentRound = document["currentRound"].ToInt32();

            this.rounds = new List<List<Action>>();

            for (int i = 0; i < document["rounds"].AsBsonArray.Count(); i++)
            {
                var roundBson = document["rounds"][i][i.ToString()];

                List<Action> round = new List<Action>();

                for (int j = 0; j < roundBson.AsBsonArray.Count(); j++)
                {
                    Action action = new Action(roundBson[j]["actionNo"].ToInt32(), roundBson[j]["actionType"].ToInt32(), roundBson[j]["player"].ToString(), roundBson[j]["sum"].ToDouble());

                    round.Add(action);
                }

                rounds.Add(round);
            }
        }


        /// <summary>
        /// Creates HandHistory Object using XML handhistory element
        /// </summary>
        /// <param name="game"> XML element of the hand history (game tag) </param>
        /// <param name="playerName"> Player name to identify player by </param>
        /// <param name="bigBlind"> Big blind size </param>
        public HandHistory(XElement game, string playerName, double bigBlind)
        {
            PokerLogic p = new PokerLogic();

            this.tableCards = new List<Card>();

            var players = game.Descendants("general").Descendants("players").Descendants("player");
            var hero = players.Where(p => p.Attribute("name").Value == playerName).First();

            double heroBlinds = double.Parse(hero.Attribute("chips").Value.Substring(1)) / bigBlind;
            double villainBlinds = double.Parse(players.Where(p => p.Attribute("name").Value != playerName).First().Attribute("chips").Value.Substring(1)) / bigBlind;
            this.heroBlindRatio = heroBlinds / (heroBlinds + villainBlinds);

            this.currentRound = -1;

            this.isDealer = hero.Attribute("dealer").Value == "1";
            this.rounds = new List<List<Action>>();

            foreach (XElement round in game.Descendants("round"))
            {
                // We ignore the first round as it is just the posting of blinds, which is always the same
                if (round.Attribute("no").Value != "0")
                {
                    List<Action> actions = new List<Action>();

                    // Player cards are dealt in round 1
                    if (round.Attribute("no").Value == "1")
                    {
                        // Get the hero's cards as strings
                        string[] heroCardsAsString = round.Descendants("cards").Single(c => c.Attribute("player").Value == playerName).Value.Split(' ');

                        this.hand = new Hand(new Card(heroCardsAsString[0]), new Card(heroCardsAsString[1]));
                    }

                    // Add each action to the actions list
                    foreach (XElement action in round.Descendants("action"))
                    {
                        actions.Add(new Action(int.Parse(action.Attribute("no").Value), int.Parse(action.Attribute("type").Value), action.Attribute("player").Value, double.Parse(action.Attribute("sum").Value.Substring(1)) / bigBlind));
                    }

                    // Check if there is only one card element (to avoid the player's cards being seen as table cards)
                    if (round.Descendants("cards").Count() == 1)
                    {
                        string[] tableCardsAsString = round.Descendants("cards").First().Value.Split(' ');

                        foreach (string cardAsString in tableCardsAsString)
                        {
                            this.tableCards.Add(new Card(cardAsString));
                        }
                    }

                    this.rounds.Add(actions);
                }
            }

            this.handStrength = p.GetHandStrength(hand, tableCards);
        }

        /// <summary>
        /// Compares the similarity of two hand histories
        /// </summary>
        /// <param name="hh"> Hand hsitory object to compare against </param>
        /// <returns> Bool indicating if the two hand histories are similar </returns>
        //public bool IsSimilar(HandHistory hh)
        //{
        //    bool isSimilar = true;

        //    if (this.hand.IsSimilarTo(hh.hand, true) && this.isDealer == hh.isDealer)
        //    {
        //        if (this.rounds.Count <= hh.rounds.Count)
        //        {
        //            for (int roundIndex = 0; roundIndex < this.rounds.Count; roundIndex++)
        //            {
        //                if(this.rounds[roundIndex].Count <= hh.rounds[roundIndex].Count)
        //                {
        //                    for (int actionIndex = 0; actionIndex < this.rounds[roundIndex].Count; actionIndex++)
        //                    {
        //                        Action action1 = this.rounds[roundIndex][actionIndex];
        //                        Action action2 = hh.rounds[roundIndex][actionIndex];

        //                        if (!action1.IsSimilarTo(action2))
        //                        {
        //                            isSimilar = false;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    isSimilar = false;
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            isSimilar = false;
        //        }
        //    }
        //    else
        //    {
        //        isSimilar = false;
        //    }

        //    return isSimilar;
        //}

        /// <summary>
        /// Gets the distance of similarity between two rounds of actions
        /// </summary>
        /// <param name="thisRound"> This hand history's round </param>
        /// <param name="otherRounds"> The set of rounds to compare against </param>
        /// <returns> int value indicating the distance of similarity between the two rounds, 0 being the closest </returns>
        public int GetRoundDistance(List<Action> thisRound, List<Action> otherRound)
        {
            // Distance of similarity between the two rounds
            int distance = 0;

            // 20% margin allowed when comparing action sums
            double margin = 0.2;

            for(int i = 0; i < thisRound.Count || i < otherRound.Count; i++)
            {
                // If "i"th action does not exists for one of the rounds, then add the max value for distance
                if(i >= thisRound.Count || i >= otherRound.Count)
                {
                    distance += 4;
                }
                else
                {
                    // If the actions are different
                    if (thisRound[i].actionType != otherRound[i].actionType)
                    {
                        distance += 3;
                    }

                    // The sums are not the same within the margin allowed, or both actions were not an all in
                    if (!((thisRound[i].sum >= (otherRound[i].sum - (otherRound[i].sum * margin)) && thisRound[i].sum <= (otherRound[i].sum + (otherRound[i].sum * margin)))
                        || (thisRound[i].actionType == 7 && otherRound[i].actionType == 7)))
                    {
                        distance += 1;
                    }
                }
            }

            return distance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theseRounds"></param>
        /// <param name="otherRounds"></param>
        /// <returns></returns>
        public int[] GetSetOfRoundsDistance(List<List<Action>> theseRounds, List<List<Action>> otherRounds)
        {
            // Int array which contains the distance between each round respectively
            int[] distance = new int[theseRounds.Count];

            for(int i = 0; i < theseRounds.Count; i++)
            {
                // If the "i"th round exists for both sets of rounds
                if(i < otherRounds.Count)
                {
                    distance[i] = this.GetRoundDistance(theseRounds[i], otherRounds[i]);
                }
                else
                {
                    // If the other rounds does not contain an "i"th round, then give it a distance value of 12
                    distance[i] = 12;
                }
            }

            return distance;
        }
    }
}
