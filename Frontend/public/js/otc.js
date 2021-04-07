let link = new PhantasmaLink("otc");
let linkVersion = 1;
let payload = 'exchange';
// Utils zone
function loginToPhantasma(providerHint) {
    $("#loginModal").modal("hide");
    console.log("Trying to login to phantasma via " + providerHint);
    link.login(
        function (success) {
            if (success) {
                $.ajax({
                    url: "login",
                    type: "post",
                    data: { token: link.token, name: link.account.name, connector: link.wallet, provider: providerHint, address: link.account.address },
                    success: function (response) {
                        // Handle correct version
                        if (link.version > 1 && link.nexus != expectedNexus) {
                            bootbox.alert("Invalid nexus, connected to " + link.nexus + " but expected " + expectedNexus);
                            logOut();
                            return;
                        }
                        
                        // Handle login success
                        changeUIs();
            	        updateOffers();
                        loadUserData();
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.log(textStatus, errorThrown);
                    },
                });
            }
        },
        linkVersion,
        "phantasma",
        providerHint
    );
}

function logOut() {
    $.ajax({
        url: "logout",
        type: "post",
        success: function (response) {
            location.reload();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(textStatus, errorThrown);
        },
    });
}

function getBalance(symbol) {
    if (!link.account) {
        return 0;
    }
    let balances = link.account.balances;
    var i;
    for (i = 0; i < balances.length; i++) {
        var entry = balances[i];
        if (entry.symbol == symbol) {
            return entry.value / Math.pow(10, entry.decimals);
        }
    }
    return 0;
}

function convertToBigInt(symbol, amount) {
    if (!link.account) {
        return 0;
    }
    let balances = link.account.balances;
    var i;
    for (i = 0; i < balances.length; i++) {
        var entry = balances[i];
        if (entry.symbol == symbol) {
            return Math.floor(amount * Math.pow(10, entry.decimals));
        }
    }
    return 0;
}

function loadUserData(){
    $("#userWalletAddress").html(link.account.address);
    $("#userSOULAmmount").html(getBalance("SOUL"));
    $("#userKCALAmmount").html(getBalance("KCAL"));
}

function changeUIs(){
    $("#btn-login").toggle();
    $("#btn-logout").toggle();
    $("#userDetails").toggle();
}

function confirmLogOut()
{
	bootbox.confirm("Do you want to disconnect from ?", function(result){ 
		if (result) {
			logOut();
		}
	});
}

function updateOffers(){
    $.ajax({
		type: 'GET',
		url: '/offers', 
		success: function(result) {
			console.log('update Offers');
			$("#offers-content").html(result);
		}
	});
}

function updateUserData(){
    if(!link.account){
        return 0;
    } 

    link.fetchAccountInfo(function(){
        loadUserData();
    });
}

// Offer zone
/**
 * Take an offer
 * @param {*} id 
 */
function takeOffer(id) {
    if (!link.account) {
        $("#loginModal").modal("show");
        return 0;
    }

    let offerInfo = $("#offer-"+id).data();
    let confirmText = "Are you sure you want to buy?<br>"+
    "<b>Order id:</b>"+id+"<br>"+
    "<b>Buying</b> " + offerInfo.sellamount + " " + offerInfo.sellsymbol + "<br>"+
    "<b>For</b> "+ offerInfo.buyamount + " " + offerInfo.buysymbol;

    bootbox.confirm(confirmText, function(result){ 
		if (result) {
            takeOfferAPI(id);
		}
	});
}

function takeOfferAPI(id){
    if (!link.account){
        return 0;
    }
    let address = String(link.account.address);

    var sb = new ScriptBuilder();
    var myScript = sb.
        allowGas(address).
        callContract("exchange", "TakeOrder", [address, 
        String(id)]).
        spendGas(address).
        endScript();
    link.sendTransaction("main", myScript, payload, (script) =>
    {
        updateUserData();
        updateOffers();
    });
}

function cancelOffer(id) {
    if (!link.account) {
        return 0;
    }
    let offerInfo = $("#offer-"+id).data();
    let confirmText = "Are you sure you want to cancel?<br>"+
    "<b>Order id:</b>"+id+"<br>"+
    "<b>Selling</b> " + offerInfo.sellamount + " " + offerInfo.sellsymbol + "<br>"+
    "<b>For</b> "+ offerInfo.buyamount + " " + offerInfo.buysymbol;
    bootbox.confirm(confirmText, function(result){ 
		if (result) {
            cancelOfferApi(id);
		}
	});
}

function cancelOfferApi(id){
    if (!link.account){
        return 0;
    }
    let address = String(link.account.address);

    var sb = new ScriptBuilder();
    var myScript = sb.
        allowGas(address).
        callContract("exchange", "CancelOTCOrder", [address, 
        String(id)]).
        spendGas(address).
        endScript();
    link.sendTransaction("main", myScript, payload, (script) =>
    {
        updateUserData();
        updateOffers();
    });
}

/**
 * Create an offer
 */
function openCreateOffer(){
    if (!link.account) {
        $("#loginModal").modal("show");
        return 0;
    }

    // Show modal create OTC
    $("#createOfferModal").modal("show");
}

function createOffer(formData){
    if (!link.account) {
        return 0;
    }

    createOfferApi(formData);
}

function createOfferApi(data){
    if (!link.account) {
        return 0;
    }
    
    let sellSymbol = String(data[1].value);
    let buySymbol = String(data[3].value);
    let sellAmount = String(convertToBigInt(sellSymbol, data[0].value));
    let buyAmmount = String(convertToBigInt(buySymbol, data[2].value));
    let address = String(link.account.address);

    console.log("sellSymbol:"+sellSymbol); // out -> sellSymbol:SOUL
    console.log("buySymbol:"+buySymbol); // out -> buySymbol:KCAL
    console.log("sellAmount:"+sellAmount); // out -> sellAmount:400000000
    console.log("buyAmmount:"+buyAmmount); // out -> buyAmmount:34340000000000   
    console.log("address:"+address); // out -> address:address:P2K6Sm1bUYGsFkxuzHPhia1AbANZaHBJV54RgtQi5q8oK34   

    var sb = new ScriptBuilder();
    var myScript = sb.
        allowGas(address).
        callContract("exchange", "OpenOTCOrder", [address, 
        sellSymbol, buySymbol, buyAmmount, sellAmount]).
        spendGas(address).
        endScript();
    
    link.sendTransaction("main", myScript, payload, (script) =>
    {
        updateUserData();
        updateOffers();
    });
}

$(document).ready(function(){
    $("#createOfferForm").submit(function(event){
        event.preventDefault();
        let data = $('form').serializeArray();
        createOffer(data);
    });
});