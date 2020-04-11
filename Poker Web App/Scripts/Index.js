//
function setPlayerCards(handAsString) {
    setCard(handAsString.substring(0, 2), document.getElementById("user-hand").getElementsByClassName("card")[0]);
    setCard(handAsString.substring(2, 4), document.getElementById("user-hand").getElementsByClassName("card")[1]);
}

//
function setAICards(handAsString) {
    setCard(handAsString.substring(0, 2), document.getElementById("ai-hand").getElementsByClassName("card")[0]);
    setCard(handAsString.substring(2, 4), document.getElementById("ai-hand").getElementsByClassName("card")[1]);
}

//
function setTableCards(cardsStringArray) {
    // For loop to remove all table cards
    var tableCards = document.getElementById("table-cards").getElementsByClassName("card");

    for (i = 0; i < tableCards.length; i++) {
        tableCards[i].getElementsByClassName("card-value")[0].innerHTML = "";
    }

    for (i = 0; i < cardsStringArray.length; i++) {
        setCard(cardsStringArray[i], document.getElementById("table-cards").getElementsByClassName("card")[i]);
    }
}

function hideHand(handElement) {
    var cards = handElement.getElementsByClassName("card");

    for (i = 0; i < cards.length; i++) {
        cards[i].getElementsByClassName("front")[0].src = "/Resources/card-back.png";
        cards[i].getElementsByClassName("card-value")[0].setAttribute("hidden", true);
        cards[i].getElementsByClassName("suit-top")[0].setAttribute("hidden", true);
        cards[i].getElementsByClassName("suit-bottom")[0].setAttribute("hidden", true);
    }
}

function showHand(handElement) {
    var cards = handElement.getElementsByClassName("card");

    for (i = 0; i < cards.length; i++) {
        cards[i].getElementsByClassName("front")[0].src = "/Resources/card-front.png";
        cards[i].getElementsByClassName("card-value")[0].removeAttribute("hidden");
        cards[i].getElementsByClassName("suit-top")[0].removeAttribute("hidden");
        cards[i].getElementsByClassName("suit-bottom")[0].removeAttribute("hidden");
    }
}

function setTableCardsVisiblilty(tableCardsElement){
    var cards = tableCardsElement.getElementsByClassName("card");

    for (i = 0; i < cards.length; i++) {
        if (cards[i].getElementsByClassName("card-value")[0].innerHTML == "") {
            
            cards[i].getElementsByClassName("front")[0].setAttribute("hidden", true);
            cards[i].getElementsByClassName("card-value")[0].setAttribute("hidden", true);
            cards[i].getElementsByClassName("suit-top")[0].setAttribute("hidden", true);
            cards[i].getElementsByClassName("suit-bottom")[0].setAttribute("hidden", true);
        }
        else {
            cards[i].getElementsByClassName("front")[0].src = "/Resources/card-front.png";
            cards[i].getElementsByClassName("front")[0].removeAttribute("hidden");
            cards[i].getElementsByClassName("card-value")[0].removeAttribute("hidden");
            cards[i].getElementsByClassName("suit-top")[0].removeAttribute("hidden");
            cards[i].getElementsByClassName("suit-bottom")[0].removeAttribute("hidden");
        }
    }
}

// Sets a cards value and suit
function setCard(cardAsString, cardElement) {
    cardElement.getElementsByClassName("card-value")[0].innerHTML = cardAsString.substring(0, 1);

    // If it is a black card
    if (cardAsString.substring(1, 2) == "s" || cardAsString.substring(1, 2) == "c") {
        cardElement.style.color = "black";

        // if it is a spade
        if (cardAsString.substring(1, 2) == "s") {
            cardElement.getElementsByClassName("suit-top")[0].src = "/Resources/spade.png";
            cardElement.getElementsByClassName("suit-bottom")[0].src = "/Resources/spade.png";
        }
        // Or a club
        else {
            cardElement.getElementsByClassName("suit-top")[0].src = "/Resources/club.png";
            cardElement.getElementsByClassName("suit-bottom")[0].src = "/Resources/club.png";
        }
    }
    // If it is a red card
    else {
        cardElement.style.color = "red";

        // if it is a heart
        if (cardAsString.substring(1, 2) == "h") {
            cardElement.getElementsByClassName("suit-top")[0].src = "/Resources/heart.png";
            cardElement.getElementsByClassName("suit-bottom")[0].src = "/Resources/heart.png";
        }
        // Or a diamond
        else {
            cardElement.getElementsByClassName("suit-top")[0].src = "/Resources/diamond.png";
            cardElement.getElementsByClassName("suit-bottom")[0].src = "/Resources/diamond.png";
        }
    }
}

function bet() {
    var actionSum = document.getElementById("action-sum").value;
    var actionType = -1;

    // Calculate the amount of money required to make a call
    var moneyForCall = gameState.ai.moneyInPotThisRound - gameState.user.moneyInPotThisRound;

    // Assume it is a bet for now
    var isBet = true; 

    for (i = 0; i < gameState.rounds[gameState.currentRound].length; i++) {
        if (gameState.rounds[gameState.currentRound][i].actionType == 5 || gameState.rounds[gameState.currentRound][i].actionType == 23 || gameState.rounds[gameState.currentRound][i].actionType == 7) {
            isBet = false;
        }
    }

    if (actionSum >= gameState.user.chips) {
        // Aciton type is set to All-In
        actionType = 7;
        actionSum = gameState.user.chips;
    }
    else if (actionSum == moneyForCall) {
        // Action type is set to Call
        actionType = 3;
    }
    else if (isBet) {
        // Otherwise if there have been no prior bets or raises, it is a bet
        actionType = 5;
    }
    else {
        // If there have been it is a raise/re-raise
        actionType = 23;
    }

    updateGameState(actionType, actionSum);
}

function check() {
    updateGameState(4, 0);
}

function fold() {
    updateGameState(0, 0);
}

// Starts a brand new game
function startNewGame() {
    updateGameState(-1, 0);
}

//
function updateGameState(actionType, actionSum) {
    // Show Loading Gif
    document.getElementById("loading-gif").removeAttribute("hidden");

    // Disable Buttons


    $.post(UpdateGameStateURL, { actionType: actionType, actionSum: actionSum, gameStateJSON: JSON.stringify(gameState) }, function (updatedGameState) {
        // Hide Loading Gif
        document.getElementById("loading-gif").setAttribute("hidden", true);

        // Re-enable Buttons

        gameState = JSON.parse(updatedGameState);
        update(updatedGameState);
    });
}

//
function update(updatedGameStateJSON) {
    var gameState = JSON.parse(updatedGameStateJSON);

    if (gameState.user.chips == 0) {
        // User has lost
        displayGameFinishedMessage(-1);
    }
    else if (gameState.ai.chips == 0) {
        // AI has lost
        displayGameFinishedMessage(1);
    }
    
    updateActionLog(gameState.rounds);

    // Update user chips
    document.getElementById("user-info").getElementsByTagName("div")[0].getElementsByClassName("chips")[0].innerHTML = gameState["user"]["chips"];

    // Update ai chips
    document.getElementById("ai-info").getElementsByTagName("div")[0].getElementsByClassName("chips")[0].innerHTML = gameState["ai"]["chips"];

    updateDealerChip(gameState.user.isDealer);
    
    // Update list of actions
    $.get(GetRoundsHTMLURL, { roundsJSON: JSON.stringify(gameState["rounds"]) }, function (html) {
        //document.getElementById("rounds").innerHTML = html;
    });

    setTableCards(JSONCardArrayToStringArray(gameState["tableCards"]));

    setTableCardsVisiblilty(document.getElementById("table-cards"));

    // Update pot
    document.getElementById("pot").getElementsByClassName("value")[0].innerHTML = gameState["pot"];

    var userHand = JSONCardArrayToStringArray(gameState["user"]["hand"]["cards"]);
    var aiHand = JSONCardArrayToStringArray(gameState["ai"]["hand"]["cards"]);

    setPlayerCards(userHand[0] + userHand[1]);
    setAICards(aiHand[0] + aiHand[1]);
}

function JSONCardArrayToStringArray(JSONCardArray) {
    var stringArray = new Array(JSONCardArray.length);

    for (i = 0; i < JSONCardArray.length; i++) {
        stringArray[i] = convertCardValue(JSONCardArray[i].value) + JSONCardArray[i].suit;
    }

    return stringArray;
}

function convertCardValue(value) {
    var newValue = value;

    if (value >= 10) {
        switch (value) {
            case 10:
                newValue = "T";
                break;
            case 11:
                newValue = "J";
                break;
            case 12:
                newValue = "Q";
                break;
            case 13:
                newValue = "K";
                break;
            case 14:
                newValue = "A";
                break;
        }
    }

    return newValue;
}

function updateActionLog(rounds) {
    var logBox = document.getElementById("log-box");
    logBox.innerHTML = "";

    for (i = 0; i < rounds.length; i++) {
        logBox.innerHTML += "<div>Round " + (i + 1) + "</div>";

        for (j = 0; j < rounds[i].length; j++) {
            logBox.innerHTML += "<div>" + (rounds[i][j]["actionNo"] + 1) + ": " + rounds[i][j]["player"] + " - " + convertBetTypeFromInt(rounds[i][j]["actionType"]) + " " + rounds[i][j]["sum"] + "</div>";
        }
    }
}

function convertBetTypeFromInt(type) {
    var stringType = "";

    switch (type) {
        case 0:
            stringType = "Fold";
            break;
        case 1:
            stringType = "Post SB";
            break;
        case 2:
            stringType = "Post BB";
            break;
        case 23:
            stringType = "Raise";
            break;
        case 3:
            stringType = "Call";
            break;
        case 4:
            stringType = "Check";
            break;
        case 5:
            stringType = "Bet";
            break;
        case 7:
            stringType = "All-In";
            break;
    }

    return stringType;
}

function updateDealerChip(userIsDealer) {
    if (userIsDealer) {
        document.getElementById("user-info").getElementsByClassName("dealer-indicator")[0].style.visibility = 'visible'
        document.getElementById("ai-info").getElementsByClassName("dealer-indicator")[0].style.visibility = 'hidden'
    }
    else {
        document.getElementById("ai-info").getElementsByClassName("dealer-indicator")[0].style.visibility = 'visible'
        document.getElementById("user-info").getElementsByClassName("dealer-indicator")[0].style.visibility = 'hidden'
    }
}

// Function for starting new game and hiding game ended screen
function finishedStartNewGame() {
    document.getElementById("game-ended-message").setAttribute("hidden", true);
    startNewGame();
}

// result == 1 for user win, result == -1 for AI win
function displayGameFinishedMessage(result) {
    document.getElementById("game-ended-message").removeAttribute("hidden");

    if (result == 1) {
        document.getElementById("game-ended-message").getElementsByClassName("win")[0].removeAttribute("hidden");
        document.getElementById("game-ended-message").getElementsByClassName("lose")[0].setAttribute("hidden", true);
    }
    else if (result == -1) {
        document.getElementById("game-ended-message").getElementsByClassName("lose")[0].removeAttribute("hidden");
        document.getElementById("game-ended-message").getElementsByClassName("win")[0].setAttribute("hidden", true);
    }
}

// Stop the "e" key being used on the action sum input
document.getElementById("action-sum").addEventListener("keydown", function (event) {
    if (event.which === 69) {
        event.preventDefault();
    }
});



//console.log(gameState);

updateActionLog(gameState.rounds);
updateDealerChip(gameState.user.isDealer);

setTableCards(JSONCardArrayToStringArray(gameState.tableCards));
setTableCardsVisiblilty(document.getElementById("table-cards"));

var userHand = JSONCardArrayToStringArray(gameState.user.hand.cards);
var aiHand = JSONCardArrayToStringArray(gameState.ai.hand.cards);

setPlayerCards(userHand[0] + userHand[1]);
setAICards(aiHand[0] + aiHand[1]);


