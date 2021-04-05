let link = new PhantasmaLink("otc");
let linkVersion = 1;

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

    bootbox.confirm("Are you sure you want to buy?", function(result){ 
		if (result) {
            $.ajax({
                url: "take/buy",
                type: "post",
                data: {address: link.account.address, uid:id},
                success: function (response) {
                    console.log("added new:"+response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log(textStatus, errorThrown);
                },
            });
		}
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

function createOffer(){
    if (!link.account) {
        return 0;
    }

    // Handle ajax
    $.ajax({
        url: "take/create",
        type: "post",
        data: {
            address: link.account.address, 
            uid:id,
            sellSymbol: "",
            sellAmount: "",
            buySymbol: "",
            buyAmmount: "",
        },
        success: function (response) {
            console.log("added new:"+response);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(textStatus, errorThrown);
        },
    });
}