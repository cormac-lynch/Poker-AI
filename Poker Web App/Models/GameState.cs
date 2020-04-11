using Action = Poker_Web_App.Poker.Action;
using System;
using System.Collections.Generic;
using Poker_Web_App.Poker;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Poker_Web_App.Models
{
    /// <summary>
    /// 
    /// </summary>
    [Bind(Exclude="cbr")]
    public class GameState
    {
        /// <summary>
        /// 
        /// </summary>
        public Player user;
        
        /// <summary>
        /// 
        /// </summary>
        public Player ai;

        /// <summary>
        /// Case Based Reasoning object for getting AIs next move, scriptignore tells the json serializer not to ignore this.
        /// (we want to ignore it because all the hand history objects will make it too big)
        /// </summary>
        [ScriptIgnore]
        public CBR cbr;

        /// <summary>
        /// 
        /// </summary>
        public int pot;

        /// <summary>
        /// 
        /// </summary>
        public const int SmallBlind = 10;

        /// <summary>
        /// 
        /// </summary>
        public const int BigBlind = 20;

        /// <summary>
        /// 
        /// </summary>
        public List<Card> tableCards;

        /// <summary>
        /// 
        /// </summary>
        public Deck deck;

        /// <summary>
        /// 
        /// </summary>
        public int currentRound;

        /// <summary>
        /// 
        /// </summary>
        public List<List<Action>> rounds;

        /// <summary>
        /// 
        /// </summary>
        public GameState()
        {
            Random rand = new Random();

            this.cbr = new CBR();

            this.user = new Player(1000, null, rand.Next(0, 2) != 0, "user", 0);
            this.ai = new Player(1000, null, !this.user.isDealer, "ai", 0);

            this.pot = 0;
            this.rounds = new List<List<Action>>();

            this.currentRound = 0;
            this.deck = new Deck();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DealHands()
        {
            this.deck.Reset();
            this.deck.Shuffle();

            if (this.user.isDealer)
            {
                this.user.hand = new Hand(deck.cards[0], deck.cards[2]);
                this.ai.hand = new Hand(deck.cards[1], deck.cards[3]);
            }
            else
            {
                this.ai.hand = new Hand(deck.cards[0], deck.cards[2]);
                this.user.hand = new Hand(deck.cards[1], deck.cards[3]);
            }

            this.deck.cards.RemoveRange(0, 4);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentRound"></param>
        public void DealTableCards(int currentRound)
        {
            // Flop
            if(currentRound == 1)
            {
                // Burn one card
                this.deck.cards.RemoveAt(0);

                // Deal three cards
                this.tableCards.Add(this.deck.cards[0]);
                this.tableCards.Add(this.deck.cards[1]);
                this.tableCards.Add(this.deck.cards[2]);
                this.deck.cards.RemoveRange(0, 3);
            }
            // Turn or River
            else if (currentRound == 2 || currentRound == 3)
            {
                // Burn one card
                this.deck.cards.RemoveAt(0);

                // Deal one card
                this.tableCards.Add(this.deck.cards[0]);
                this.deck.cards.RemoveAt(0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartNewHand()
        {
            this.pot = 0;
            this.rounds = new List<List<Action>>();
            this.tableCards = new List<Card>();
            this.currentRound = 0;
            this.user.isDealer = !this.user.isDealer;
            this.ai.isDealer = !this.ai.isDealer;

            this.ai.totalMoneyInPot = 0;
            this.ai.moneyInPotThisRound = 0;
            this.user.totalMoneyInPot = 0;
            this.user.moneyInPotThisRound = 0;

            // Deal cards to players (includes deck reset and shuffle)
            this.DealHands();

            this.TakeBlinds();

            // dealer moves first pre flop
            if(this.ai.isDealer)
            {
                this.MakeAIMove();
            }
        }

        /// <summary>
        /// Advances hand using the user's action
        /// </summary>
        /// <param name="userAction"> Action of the user </param>
        /// <returns> The AIs action, if the user didnt fold </returns>
        public GameState AdvanceHand(Action userAction)
        {
            this.rounds[this.currentRound].Add(userAction);
            this.HandleAction(userAction, user);

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        private void TakeBlinds()
        {
            // Add preflop round
            this.rounds.Add(new List<Action>());

            bool allInCalled = false;

            if (this.user.isDealer)
            {
                if (this.user.chips > SmallBlind)
                {
                    this.pot += SmallBlind;
                    
                    this.user.moneyInPotThisRound += SmallBlind;
                    this.user.chips -= SmallBlind;

                    // Add posting of small blind
                    this.rounds[0].Add(new Action(this.rounds[currentRound].Count, 1, "user", (double)SmallBlind));
                }
                else
                {
                    this.pot += this.user.chips;
                    
                    this.user.moneyInPotThisRound += this.user.chips;
                    this.user.chips -= this.user.chips;

                    // Add all in
                    this.rounds[0].Add(new Action(this.rounds[currentRound].Count, 7, "user", (double)this.user.moneyInPotThisRound));

                    allInCalled = true;
                }

                if (this.ai.chips > BigBlind)
                {
                    this.pot += BigBlind;

                    this.ai.moneyInPotThisRound += BigBlind;
                    this.ai.chips -= BigBlind;

                    // Add posting of big blind
                    this.rounds[0].Add(new Action(this.rounds[currentRound].Count, 2, "ai", (double)BigBlind));

                    if (allInCalled)
                    {
                        this.AllIn();
                    }
                }
                else
                {
                    this.pot += this.ai.chips;

                    this.ai.moneyInPotThisRound += this.ai.chips;
                    this.ai.chips -= this.ai.chips;

                    // Add all in
                    this.rounds[0].Add(new Action(this.rounds[currentRound].Count, 7, "ai", this.ai.moneyInPotThisRound));

                    if (this.ai.moneyInPotThisRound < SmallBlind)
                    {
                        this.AllIn();
                    }
                }
            }
            else
            {
                if (this.ai.chips > SmallBlind)
                {
                    this.pot += SmallBlind;

                    this.ai.moneyInPotThisRound += SmallBlind;
                    this.ai.chips -= SmallBlind;

                    // Add posting of small blind
                    this.rounds[0].Add(new Action(this.rounds[currentRound].Count, 1, "ai", (double)SmallBlind));
                }
                else
                {
                    this.pot += this.ai.chips;

                    this.ai.moneyInPotThisRound += this.ai.chips;
                    this.ai.chips -= this.ai.chips;

                    // Add all in
                    this.rounds[0].Add(new Action(this.rounds[currentRound].Count, 7, "ai", this.ai.moneyInPotThisRound));

                    allInCalled = true;
                }

                if (this.user.chips > BigBlind)
                {
                    this.pot += BigBlind;

                    this.user.moneyInPotThisRound += BigBlind;
                    this.user.chips -= BigBlind;

                    // Add posting of big blind
                    this.rounds[0].Add(new Action(this.rounds[currentRound].Count, 2, "user", (double)BigBlind));

                    if (allInCalled)
                    {
                        this.AllIn();
                    }
                }
                else
                {
                    this.pot += this.user.chips;

                    this.user.moneyInPotThisRound += this.user.chips;
                    this.user.chips -= this.user.chips;

                    // Add all in
                    this.rounds[0].Add(new Action(this.rounds[currentRound].Count, 7, "user", (double)this.user.moneyInPotThisRound));

                    this.AllIn();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"> -1 = AI win | 0 = Tie | 1 = User win </param>
        public void HandFinished(int result)
        {
            switch(result)
            {
                // AI win
                case -1:
                    this.ai.chips += pot;
                    break;

                // Tie
                case 0:
                    int remainder = this.pot % 2;

                    user.chips += (this.pot - remainder) / 2;
                    ai.chips += (this.pot - remainder) / 2;

                    if (user.isDealer)
                    {
                        user.chips += remainder;
                    }
                    else
                    {
                        ai.chips += remainder;
                    }

                    break;

                // User win
                case 1:
                    this.user.chips += pot;
                    break;
            }

            if (user.chips > 0 && ai.chips > 0)
            {
                this.StartNewHand();
            }
            else
            {
                // End game
            }
        }

        /// <summary>
        /// Advances to the next round or finished the hand if it is currently the final round
        /// </summary>
        public void AdvanceToNextRound()
        {
            // If it is the final round
            if (this.currentRound == 3)
            {
                PokerLogic p = new PokerLogic();

                this.HandFinished(p.GetShowdownWinner(this.user.hand, this.ai.hand, this.tableCards));
            }
            else
            {
                // Advance to next round
                this.currentRound++;

                this.ai.totalMoneyInPot += this.ai.moneyInPotThisRound;
                this.ai.moneyInPotThisRound = 0;

                this.user.totalMoneyInPot += this.user.moneyInPotThisRound;
                this.user.moneyInPotThisRound = 0;

                this.rounds.Add(new List<Action>());

                this.DealTableCards(currentRound);

                if (this.user.isDealer)
                {
                    this.MakeAIMove();
                }
            }
        }

        /// <summary>
        /// Gets the AIs next action and updates the gamestate according to this action
        /// </summary>
        public void MakeAIMove()
        {
            // Create a HandHistory object from the current gamestate
            HandHistory currentHand = new HandHistory();
            currentHand.hand = this.ai.hand;
            currentHand.handStrength = new PokerLogic().GetHandStrength(this.ai.hand, this.tableCards);
            currentHand.heroBlindRatio = (double)this.ai.chips / this.user.chips;
            currentHand.isDealer = this.ai.isDealer;
            currentHand.rounds = this.rounds;

            // The actons in the data are in number of big blinds (e.g. a bet size of 1BB instead of 20) so adjust accordingly
            currentHand.rounds.ForEach(round => round.ForEach(action => action.sum /= BigBlind));
            currentHand.tableCards = this.tableCards;

            // Get the AIs next move
            Action aiAction = cbr.GetNextAction(currentHand);
         
            // The new acton will be in number of big blinds (e.g. a bet size of 1BB instead of 20) so adjust accordingly
            aiAction.sum *= BigBlind;

            // Change the actions bet sizing back
            currentHand.rounds.ForEach(round => round.ForEach(action => action.sum *= BigBlind));

            // If the action type was a call
            if (aiAction.actionType == 3)
            {
                if (this.rounds[this.currentRound][this.rounds[this.currentRound].Count - 1].actionType == 7)
                {
                    // Change the sum of the action to be the amount required to make a call in the current situation
                    aiAction.sum = user.moneyInPotThisRound - ai.moneyInPotThisRound;
                }
                else
                {
                    // Change the sum of the action to be the amount required to make a call in the current situation
                    aiAction.sum = this.rounds[this.currentRound][this.rounds[this.currentRound].Count - 1].sum - ai.moneyInPotThisRound;
                }
            }
            // If the action was an all-in
            else if (aiAction.actionType == 7)
            {
                // Change the sum of the action to be the amount required for the AI to go all in
                aiAction.sum = this.ai.chips;
            }

            // If there have been no actions in the current round yet
            if (this.rounds[currentRound].Count == 0)
            {
                // Set the action number to be one greater than the final action of the last round
                aiAction.actionNo = this.rounds[currentRound - 1][this.rounds[currentRound - 1].Count - 1].actionNo + 1;
            }
            else
            {
                // Set the action number to be one greater than the most recent action in the current round
                aiAction.actionNo = this.rounds[currentRound][this.rounds[currentRound].Count - 1].actionNo + 1;
            }

            this.rounds[this.currentRound].Add(aiAction);
            this.HandleAction(aiAction, ai);
        }

        /// <summary>
        /// Function for finishing a game once an all in has been called
        /// </summary>
        public void AllIn()
        {
            this.currentRound++;

            while (this.currentRound < 4)
            {
                this.DealTableCards(this.currentRound);
                this.currentRound++;
            }

            PokerLogic p = new PokerLogic();

            this.HandFinished(p.GetShowdownWinner(this.user.hand, this.ai.hand, this.tableCards));
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="player"></param>
        public void HandleAction(Action action, Player player)
        {
            // In the handhistories made from the data, small and big blinds are included as actions in the first round but not in our hand histories
            int roundIndexDifference = currentRound == 0 ? this.rounds[currentRound].Count - 2 : 0;

            switch (action.actionType)
            {
                // Fold
                case 0:
                    if(player.name.Equals("user"))
                    {
                        this.HandFinished(-1);
                    }
                    else
                    {
                        this.HandFinished(1);
                    }

                    break;

                // Raise
                case 23:
                    player.chips -= (int)action.sum - player.moneyInPotThisRound;
                    this.pot += (int)action.sum - player.moneyInPotThisRound;

                    player.moneyInPotThisRound += (int)action.sum - player.moneyInPotThisRound;

                    if (player.name.Equals("user"))
                    {
                        this.MakeAIMove();
                    }

                    break;

                // Call
                case 3:
                    player.chips -= (int)action.sum;
                    player.moneyInPotThisRound += (int)action.sum;
                    this.pot += (int)action.sum;

                    // If an all-in was called
                    if (this.rounds[currentRound].Count >= 2 && this.rounds[currentRound][this.rounds[currentRound].Count - 2].actionType == 7)
                    {
                        this.AllIn();
                    }
                    else if (!(this.currentRound == 0 && action.actionNo == 2))
                    {
                        this.AdvanceToNextRound();
                    }
                    // The action was the player calling the big blind
                    else if (player.name == "user")
                    {
                        this.MakeAIMove();
                    }

                    break;

                // Check 
                case 4:
                    // If last action was a check
                    if ((this.rounds[currentRound].Count > 1 && this.rounds[currentRound][(this.rounds[currentRound].Count - 1) - roundIndexDifference].actionType == 4) ||
                        (action.actionNo == 3 && this.currentRound == 0 && this.rounds[currentRound][2].actionType == 3))
                    {
                        this.AdvanceToNextRound();
                    }
                    else
                    {
                        // If it was the user who checked
                        if (player.name.Equals("user"))
                        {
                            this.MakeAIMove();
                        }
                    }
                    break;

                // Bet
                case 5:
                    this.pot += (int)action.sum;
                    player.chips -= (int)action.sum;
                    player.moneyInPotThisRound += (int)action.sum;

                    // If it was the user who bet
                    if (player.name.Equals("user"))
                    {
                        this.MakeAIMove();
                    }
                    break;

                // All-in
                case 7:
                    player.chips -= (int)action.sum;
                    player.moneyInPotThisRound += (int)action.sum;
                    this.pot += (int)action.sum;

                    
                    Action lastAction = this.rounds[currentRound][this.rounds[currentRound].Count - 2 - roundIndexDifference];

                    // If the last action was a bet or raise
                    if (lastAction.actionType == 23 || lastAction.actionType == 5)
                    {
                        if (lastAction.sum >= action.sum)
                        {
                            this.AllIn();
                        }
                        else
                        {
                            // If the user made the all in
                            if (player.name.Equals("user"))
                            {
                                this.MakeAIMove();
                            }
                        }
                    }
                    // If the all in was calling another all in
                    else if (lastAction.actionType == 7)
                    {
                        this.AllIn();
                    }
                    else if (player.name.Equals("user"))
                    {
                        this.MakeAIMove();
                    }

                    break;

            }
        }
    }
}