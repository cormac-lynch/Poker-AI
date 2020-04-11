using MongoDB.Bson;
using Poker_Web_App.Models;
using Poker_Web_App.Poker;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Action = Poker_Web_App.Poker.Action;

namespace Poker_Web_App.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        public GameState gameState;

        /// <summary>
        /// Directs to the index page, with a hand history as the Model
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            this.gameState = new GameState();

            this.gameState.StartNewHand();

            return View("Index", this.gameState);
        }


        #region Methods

        /// <summary>
        /// Updates the game state for the front end
        /// </summary>
        /// <param name="actionType"> Type of action the user is making</param>
        /// <param name="actionSum">  Sum of the users action</param>
        /// <param name="gameStateJSON"> JSON string of the curent gameststate</param>
        /// <returns> new gamestate in JSON </returns>
        public string UpdateGameState(int actionType, int actionSum, string gameStateJSON)
        {
            // Negative actiontype indicates the user has selected to restart the game
            if(actionType < 0)
            {
                this.gameState = new GameState();

                this.gameState.StartNewHand();
            }
            else
            {
                this.gameState = (GameState)new JavaScriptSerializer().Deserialize(gameStateJSON, typeof(GameState));

                // Get the number of the next action
                int actionNo = 0;
                if (this.gameState.rounds[this.gameState.currentRound].Count > 0)
                {
                    actionNo = this.gameState.rounds[this.gameState.currentRound].Last().actionNo + 1;
                }
                else
                {
                    actionNo = this.gameState.rounds[this.gameState.currentRound - 1].Last().actionNo + 1;
                }

                Action newUserAction = new Action(actionNo, actionType, "user", actionSum);

                this.gameState.AdvanceHand(newUserAction);
            }

            return new JavaScriptSerializer().Serialize(this.gameState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetRoundsHTML(string roundsJSON)
        {
            List<List<Action>> rounds = (List<List<Action>>)new JavaScriptSerializer().Deserialize(roundsJSON, typeof(List<List<Action>>));

            string html = "";

            foreach (List<Action> round in rounds)
            {
                html += "<div>Round:</div>";

                foreach (Action action in round)
                {
                    html += "<div><div class='inline-block'>action: </div><div class='inline-block'>" + action.actionType + " | </div><div class='inline-block'>" + action.sum + "</div class='inline-block'></div>";
                }
            }

            return html;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableCardsJSON"></param>
        /// <returns></returns>
        public string GetTableCardsHTML(string tableCardsJSON)
        {
            List<Card> tableCards = (List<Card>)new JavaScriptSerializer().Deserialize(tableCardsJSON, typeof(List<Card>));

            string html = "";

            foreach (Card c in tableCards)
            {
                html += "<div>" + c.ToString() + "</div>";
            }

            return html;
        }



        public int GetPot()
        {
            return this.gameState.pot;
        }

        public int GetUserChips()
        {
            return this.gameState.user.chips;
        }

        public int GetAIChips()
        {
            return this.gameState.ai.chips;
        }

        public string GetUserHand()
        {
            return this.gameState.user.hand.ToString();
        }

        public string GetAIHand()
        {
            return this.gameState.ai.hand.ToString();
        }

        #endregion
    }
}