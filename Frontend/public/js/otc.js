let link = new PhantasmaLink("otc");
let linkVersion = 1;
let payload = 'exchange';
let offerInterval;
let myProvider = ""; 
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
                        myProvider = providerHint;
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

function connectToWallet(providerHint){
    link.login(
        function (success) {
            if ( success ){
                // Handle login success
                changeUIs();
                updateOffers();
                loadUserData();
            }else {
                //link.hideModal();
                logOut();
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
	bootbox.confirm({
        title: "Logout",
        message: "Do you want to disconnect from "+myProvider+"?",
        buttons: {
            confirm: {
                label: 'Logout',
                className: 'btn-success'
            },
            cancel : {
                label : 'Cancel',
                className: 'btn-danger'
            }
        },
        callback: function(result){ 
            if (result) {
                logOut();
            }
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

//function getOffersAPI(){
//    if(!link.account){
//        return 0;
//    } 
//
//    var sb = new ScriptBuilder();
//    var myScript = sb.
//        callContract("exchange", "GetOTC", []).
//        endScript();
//
//    link.invokeRawScript("main", myScript, payload, (script) =>
//    {
//        console.log(script);
//        console.log(script.result);
//        console.log(script.result.result);
//    });
//}

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
    let confirmText = "Are you sure you wish to take this OTC offer?<br>"+
    "<b>Order ID:</b> "+id+"<br>"+
    "<b>Buying:</b> " + offerInfo.sellamount + " " + offerInfo.sellsymbol + "<br>"+
    "<b>For:</b> "+ offerInfo.buyamount + " " + offerInfo.buysymbol;

    bootbox.confirm({
        title: "Take Order", 
        message: confirmText,
        buttons: {
            confirm: {
                label: 'Take',
                className: 'btn-success'
            },
            cancel : {
                label : 'Cancel',
                className: 'btn-danger'
            }
        },
        callback: function(result){ 
            if (result) {
                takeOfferAPI(id);
            }
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
    let confirmText = "Are you sure you wish to cancel this OTC offer?<br>"+
    "<b>Order ID:</b> "+id+"<br>"+
    "<b>Selling:</b> " + offerInfo.sellamount + " " + offerInfo.sellsymbol + "<br>"+
    "<b>For:</b> "+ offerInfo.buyamount + " " + offerInfo.buysymbol;
    bootbox.confirm({
        title: "Cancel OTC offer", 
        message: confirmText,
        buttons: {
            confirm: {
                label: 'Cancel',
                className: 'btn-success'
            },
            cancel : {
                label : 'Exit',
                className: 'btn-danger'
            }
        },
        callback: function(result){ 
            if (result) {
                cancelOfferApi(id);
            }
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

var buyEnabled = false;
var sellEnabled = false;

$(document).ready(function(){
    $("#createOfferButton").prop( "disabled", true );

    $("#sellSymbol").on("change", function(e) {
        let value = $(this).val();
        
        if (value == "KCAL" || value == "SOUL")
        {
            sellEnabled = true;
            if (buyEnabled)
            {
                if ( value != $("#buySymbol").val() ){
                    // Enable Create offer button
                    $("#createOfferButton").prop( "disabled", false );
                }else {
                    $("#createOfferButton").prop( "disabled", true );
                }
            }
        }else {
            sellEnabled = false;
            $("#createOfferButton").prop( "disabled", true );
        }
    });

    $("#buySymbol").on("change", function(e) {
        let value = $(this).val();
        if (value == "KCAL" || value == "SOUL")
        {
            buyEnabled = true;
            
            if (sellEnabled)
            {
                if ( value != $("#sellSymbol").val() ){
                    // Enable Create offer button
                    $("#createOfferButton").prop( "disabled", false );
                }else {
                    $("#createOfferButton").prop( "disabled", true );
                }
            }
        }else {
            buyEnabled = false;
            $("#createOfferButton").prop( "disabled", true );
        }
    });

    $("#createOfferForm").submit(function(event){
        event.preventDefault();
        let data = $('form').serializeArray();
        createOffer(data);
    });
});