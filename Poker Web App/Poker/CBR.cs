using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Poker_Web_App.Poker
{
    /// <summary>
    /// Class for storing hand histories and getting the AI's next action using Case Based Reasoning
    /// </summary>
    public class CBR
    {
        /// <summary>
        /// List of hand histories
        /// </summary>
        public List<HandHistory> handHistories;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="folderName"> Name of the folder to get hand histories from </param>
        public CBR()
        {
            //var curDir = Directory.GetCurrentDirectory();

            // Pass negative quantity to load all hands
            this.handHistories = LoadHands(-1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quantity"></param>
        public CBR(int quantity)
        {
            this.handHistories = LoadHands(quantity);
        }

        /// <summary>
        /// Creates the CBR object by loading hands from XML files specified
        /// </summary>
        /// <param name="folderName"></param>
        public CBR(string folderName)
        {
            this.handHistories = LoadHands(folderName, -1);
        }

        /// <summary>
        /// Loads all hands from the database
        /// </summary>
        /// <param name="quantity"> pass a negataive value to not limit the quantity </param>
        /// <returns> List of hand history objects </returns>
        public List<HandHistory> LoadHands(int quantity)
        {
            List<HandHistory> handHistories = new List<HandHistory>();

            // Waits for the async retrieval of documents to finish before continuing
            var documents = Task.Run(async () => await GetDocumentsAsync()).Result;

            foreach (var document in documents)
            {
                handHistories.Add(new HandHistory(document));
            }

            return handHistories;
        }

        /// <summary>
        /// Loads all hands from the folder provided
        /// </summary>
        /// <param name="folderName"> Name of the folder (currently has to be in the debug folder) </param>
        /// <param name="quantity"> pass a negative value to not limit the quantity </param>
        /// <returns> List of hand history objects </returns>
        public static List<HandHistory> LoadHands(string folderName, int quantity)
        {
            List<HandHistory> handHistories = new List<HandHistory>();

            string[] filePaths = Directory.GetFiles(folderName, "*.txt", SearchOption.TopDirectoryOnly);

            // For each file in the folder
            foreach (string path in filePaths)
            {
                string fileName = path;

                try
                {
                    XDocument document = XDocument.Load(fileName);

                    // Initialize big blind value to zero
                    double bigBlind = 0.0;

                    // Loop through each hand history 
                    foreach (XElement game in document.Descendants("game"))
                    {
                        // Loop through each round
                        foreach (XElement round in game.Descendants("round"))
                        {
                            // Wait for round 1, as this is when cards are dealt to players
                            if (round.Attribute("no").Value.Equals("1"))
                            {
                                // Loop through cards
                                foreach (XElement cards in round.Descendants("cards"))
                                {
                                    // If the hand is visible
                                    if (!cards.Value.Equals("X X"))
                                    {
                                        // If the bigblind has not been assigned yet
                                        if (bigBlind == 0.0)
                                        {
                                            // Get the big blind from the XML file
                                            bigBlind = double.Parse(document.Descendants("general").Descendants("gametype").First().Value.Split('/')[1].Substring(1));
                                        }

                                        // Find the players name by matching up the hand from cards with their dealt hand
                                        string playerName = game.Descendants("round").Where(r => r.Attribute("no").Value == "1").Descendants("cards").Single(c => c.Value == cards.Value).Attribute("player").Value;

                                        // Create a new HandHistory object
                                        HandHistory hh = new HandHistory(game, playerName, bigBlind);

                                        // Add HandHistory to the list
                                        handHistories.Add(hh);

                                        if (handHistories.Count == quantity && quantity > 0)
                                        {
                                            return handHistories;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (XmlException)
                {
                    // If it is not an XML file
                }
            }

            return handHistories;
        }

        /// <summary>
        /// Gets a list of BSON documents from the mongodb asynchronously
        /// </summary>
        /// <returns> list of BSON documents </returns>
        public async Task<List<BsonDocument>> GetDocumentsAsync()
        {
            MongoClient dbClient = new MongoClient(Config.Config.dbConnectionString);

            var database = dbClient.GetDatabase(Config.Config.dbName);
            var collection = database.GetCollection<BsonDocument>(Config.Config.dbCollectionName);

            // Asynchronously get the documents
            var documents = await collection.Find(_ => true).ToListAsync();
            
            return documents;
        }

        /// <summary>
        /// Gets the AIs next action using CBR
        /// </summary>
        /// <param name="currentHand"> HandHistory object describing the current hand </param>
        /// <returns> Returns AIs next action based on the handhistory data, sums need to be adjusted for calls and all ins</returns>
        public Action GetNextAction(HandHistory currentHand)
        {
            Action nextAction = new Action();
            nextAction.player = "ai";

            // Get all nearest neighbors
            List<KeyValuePair<HandHistory, double>> nearestNeigbhors = this.GetKNearestNeighbour(currentHand, -1);

            // The index of the round to find the similar action in
            int roundIndex = currentHand.rounds.Count - 1;

            // The index of the similar action in the round
            int actionIndex = currentHand.rounds[roundIndex].Count;

            // If it is pre flop
            if (roundIndex == 0)
            {
                // HandHistories from XML files have the small and big blind removed
                actionIndex -= 2;
            }

            List<Action> similarActions = new List<Action>();

            // Find each hand history that is similar to this one
            foreach (KeyValuePair<HandHistory, double> hh in nearestNeigbhors)
            {
                if (hh.Key.rounds.Count >= roundIndex + 1)
                {
                    if (hh.Key.rounds[roundIndex].Count > actionIndex)
                    {
                        // Add the relevent action to the list
                        similarActions.Add(hh.Key.rounds[roundIndex][actionIndex]);
                    }
                }
            }
            if(similarActions.Count > 10)
            {
                similarActions.RemoveRange(10, similarActions.Count - 10);
            }

            // Set the type of the AIs next action to the most common action in the similar hands
            nextAction.actionType = similarActions.GroupBy(a => a.actionType).OrderByDescending(a => a.Count()).First().Key;

            // Get a list of all similar sums and sort by count descending
            var sums = similarActions.Where(a => a.actionType == nextAction.actionType).GroupBy(a => a.sum).OrderByDescending(a => a.Count());

            // Set the sum of the AIs next action to the most common sum in the similar hands
            nextAction.sum = similarActions.Where(a => a.actionType == nextAction.actionType).GroupBy(a => a.sum).OrderByDescending(a => a.Count()).First().Key;

            return nextAction;
        }

        /// <summary>
        /// Gets the hand hsitory objects and sorts them by most similar using k-NN
        /// </summary>
        /// <param name="hh"> hand history describing the current situation</param>
        /// <param name="k"> value to indicate how many hists you want returned, pass negative value if you want all of them</param>
        /// <returns> List of Kvp of handhistory, and value indicating similarity (smaller value = more similar) </returns>
        public List<KeyValuePair<HandHistory, double>> GetKNearestNeighbour(HandHistory thisHandHistory, int k)
        {
            // <HandHistory, Distance>
            Dictionary<HandHistory, double> nearestNeighbours = new Dictionary<HandHistory, double>();

            // Filter out by dealer status
            List<HandHistory> searchHistories = this.handHistories.Where(hh => hh.isDealer == thisHandHistory.isDealer).ToList();

            double[] maxRoundDistance = new double[4];

            // Initialise to zero
            for (int i = 0; i < maxRoundDistance.Length; i++)
            {
                maxRoundDistance[i] = 0;
            }
            
            // Get the max rounds distance which occurs for each round in all hand histories
            foreach (HandHistory hh in searchHistories)
            {
                for (int i = 0; i < thisHandHistory.rounds.Count && i < hh.rounds.Count; i++)
                {
                    int temp = thisHandHistory.GetRoundDistance(thisHandHistory.rounds[i], hh.rounds[i]);
                    maxRoundDistance[i] = temp > maxRoundDistance[i] ? temp : maxRoundDistance[i];

                }
            }

            foreach (HandHistory hh in searchHistories)
            {
                // Set max values for normalization
                const int MaxHandDistance = 20;
                const int MaxStrengthDistance = 9;
                const double MaxBlindRatio = 1;

                // Calculate distances and normalise

                // Get the distance between each hand (Weighted 12.5%)
                double handDistance = (thisHandHistory.hand.GetDistanceTo(hh.hand) / MaxHandDistance) * 0.125;

                // Get the distance between hand strengths (Weighted 15%)
                double strengthDistance = Math.Abs((double)(thisHandHistory.handStrength.strength - hh.handStrength.strength) / MaxStrengthDistance) * 0.15;

                // Get the distance between blind ratios (Weighted 2.5%)
                double blindRatioDistance = Math.Abs((thisHandHistory.heroBlindRatio - hh.heroBlindRatio) / MaxBlindRatio) * 0.025;

                // Get the distance between rounds(Weighted 70%)
                int[] roundsDistance = thisHandHistory.GetSetOfRoundsDistance(thisHandHistory.rounds, hh.rounds);
                double[] normalizedRoundsDistance = new double[roundsDistance.Length];

                // For each round
                for(int i = 0; i < roundsDistance.Length; i++)
                {
                    if (i == thisHandHistory.currentRound)
                    {
                        // 40%/70% weighting goes to the current round
                        normalizedRoundsDistance[i] = Math.Abs(roundsDistance[i] / maxRoundDistance[i]) * .4;
                    }
                    else
                    {
                        // Remaining 30%/70% is split between the remaining rounds
                        normalizedRoundsDistance[i] = Math.Abs(roundsDistance[i] / maxRoundDistance[i]) * (.3 / (thisHandHistory.rounds.Count - 1));
                    }
                }

                // Euclidean distance formula
                double distance = (handDistance * handDistance) + (strengthDistance * strengthDistance) + (blindRatioDistance * blindRatioDistance);
               
                foreach (double roundDistance in normalizedRoundsDistance)
                {
                    distance += roundDistance * roundDistance;
                }
                
                distance = Math.Sqrt(distance);

                nearestNeighbours.Add(hh, distance);
            }

            List<KeyValuePair<HandHistory, double>> orderedNearestNeighbors = nearestNeighbours.ToList();

            // Sort by most simliar hand histories
            orderedNearestNeighbors.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            // If k is negative dont limit the results
            if (k <= 0 || orderedNearestNeighbors.Count < k)
            {
                return orderedNearestNeighbors;
            }
            else
            {
                return orderedNearestNeighbors.GetRange(0, k);
            }
        }
    }
}
