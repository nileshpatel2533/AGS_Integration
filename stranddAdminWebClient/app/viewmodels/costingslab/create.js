define(['durandal/system', 'plugins/router', 'durandal/app', 'roadzen/core-ui', 'roadzen/core-security', 'knockout'], function (system, router, app, ui, security, ko) {


    //Internal & Protected Variables
    var MobileServiceClient = WindowsAzure.MobileServiceClient;

    //add jayesh
    var mslab = new MobileServiceClient(settings.liveDataService(), settings.liveAppKey());

    var identifierGUID = function () { return settings.liveBaseCompanyGUID(); };
    var serviceType = ko.observable();
    var status = ko.observable();
    var baseCharge = ko.observable();
    var baseKilometersFloor = ko.observable();
    var extraKilometersCharge = ko.observable();



    //ViewModel State Behavior
    var stateLoading = ko.observable("CostingSlab List Loading...");

    //SCM Access Settings

    var qs = {};

    var headers = { "z-xumo-auth": security.getToken() };



    var setRootTocostingslab = function () {

        router.navigate('costingslab');

    };

    var costingslab = {

        mslab: mslab,
        identifierGUID: identifierGUID,
        serviceType: serviceType,
        status: status,
        baseCharge: baseCharge,
        baseKilometersFloor: baseKilometersFloor,
        extraKilometersCharge: extraKilometersCharge,
        setRootTocostingslab: setRootTocostingslab,
        getdataslab: getdataslab,
        attemptcreate: function () { getdataslab(identifierGUID(), serviceType(), status(), baseCharge(), baseKilometersFloor(), extraKilometersCharge()).done(function (result) { setRootTocostingslab(); }); }

    };

    return costingslab;






    function getdataslab(identifierGUID, serviceType, status, baseCharge, baseKilometersFloor, extraKilometersCharge) {

        costingslab.mslab.invokeApi("costingslabs/new", {
            method: "post",
            body: { "identifierGUID": identifierGUID, "serviceType": serviceType, "status": status, "baseCharge": baseCharge, "baseKilometersFloor": baseKilometersFloor, "extraKilometersCharge": extraKilometersCharge }
        }).done(function (result) {

            alert('Create SuccessFull !');

            setRootTocostingslab();


        });

    };

});



