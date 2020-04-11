using MongoDB.Bson;
using MongoDB.Driver;
using Poker_Web_App.Config;
using Poker_Web_App.Poker;
using System.Collections.Generic;

namespace Poker_MongoDB_Insert
{
    class Program
    {
        static void Main(string[] args)
        {
            MongoClient dbClient = new MongoClient(Config.dbConnectionString);

            var database = dbClient.GetDatabase(Config.dbName);
            var collection = database.GetCollection<BsonDocument>(Config.dbCollectionName);

            // Create CBR object from the hand history data
            CBR cbr = new CBR(Config.handsFolderPath);

            // Create a BSON document from each hand history, and store it in the collection
            foreach (HandHistory hh in cbr.handHistories)
            {
                var roundsBsonArray = new BsonArray();

                foreach (List<Action> round in hh.rounds)
                {
                    var roundBsonArray = new BsonArray();

                    foreach (Action action in round)
                    {
                        roundBsonArray.Add(new BsonDocument {
                            {  "actionNo" , action.actionNo },
                            {  "actionType" , action.actionType },
                            {  "player" , action.player },
                            {  "sum" , action.sum }
                        });
                    }

                    roundsBsonArray.Add(new BsonDocument {
                        { hh.rounds.IndexOf(round).ToString() , roundBsonArray}
                    });

                }

                var tableCardsArray = new BsonArray();

                foreach (Card c in hh.tableCards)
                {
                    tableCardsArray.Add(new BsonDocument { { "value", c.value }, { "suit", c.suit } });
                }

                var significantCardsArray = new BsonArray();

                foreach (Card c in hh.handStrength.significantCards)
                {
                    if (!ReferenceEquals(c, null))
                    {
                        significantCardsArray.Add(new BsonDocument { { "value", c.value }, { "suit", c.suit } });
                    }
                }

                var document = new BsonDocument {
                    { "hand",
                        new BsonArray {
                            new BsonDocument { { "value", hh.hand.cards[0].value }, { "suit", hh.hand.cards[0].suit } },
                            new BsonDocument { { "value", hh.hand.cards[1].value }, { "suit", hh.hand.cards[1].suit } }
                        }
                    },

                    { "handStrength",
                        new BsonDocument {
                            { "strength", hh.handStrength.strength },
                            { "significantCards", significantCardsArray }
                        }
                    },

                    { "isDealer", hh.isDealer },

                    { "tableCards", tableCardsArray },

                    { "rounds", roundsBsonArray},

                    { "currentRound", hh.currentRound },

                    { "heroBlindRatio", hh.heroBlindRatio}
                };

                // Insert the document into the collection
                collection.InsertOne(document);

            }
        }

        // <summary>
        /// Loads all hands from the folder provided
        /// </summary>
        /// <param name="folderName"> Name of the folder (currently has to be in the debug folder) </param>
        /// <param name="quantity"></param>
        /// <returns> List of hand history objects </returns>
        public static List<HandHistory> LoadHands(int quantity)
        {
            List<HandHistory> handHistories = new List<HandHistory>();

            var getDocumentsTask = GetDocumentsAsync();

            getDocumentsTask.Wait();

            var documents = getDocumentsTask.Result;

            return handHistories;
        }

        public static async System.Threading.Tasks.Task<List<BsonDocument>> GetDocumentsAsync()
        {
            MongoClient dbClient = new MongoClient("mongodb://localhost");

            var database = dbClient.GetDatabase("poker_ai");
            var collection = database.GetCollection<BsonDocument>("hand_histories");

            var documents = await collection.Find(_ => true).ToListAsync();

            return documents;
        }
    }
}
