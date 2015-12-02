define(['durandal/system', 'plugins/router', 'durandal/app', 'app-settings', 'roadzen/core-security', 'services/dataContext', 'services/incidentHubAdapter', 'viewmodels/incidents', 'knockout'], function (system, router, app, settings, security, dataContext, incidentHubAdapter, incidents, ko) {


    //Settings for Details
    var indexStatusCode = settings.getStatusCodeIndex();    

    //Main View Data Compute
    var currentIncident = ko.computed(function () {
        var incident = incidents.currentIncident();
        return incident;
    });



    var indexCurrentStatusCodes = ko.computed(function () {
        if (currentIncident() == null) { return settings.getStatusCodeIndex(); }
        else {
            currentSequence = currentIncident().statusDisplayObj().sequence;
            system.log(["Current Sequence...", currentSequence]);
            selectionOutput = settings.getStatusCodeIndex().filter(function (o) {
                if (currentSequence > 0 && currentSequence < 100) {
                    if (currentSequence == 1) { return ((o.sequence >= currentSequence && o.sequence <= 2) || o.sequence == -100) }
                    else { return (o.sequence >= currentSequence || o.sequence == -100) }
                }
                if (currentSequence == 100) { return (o.sequence == currentSequence || o.sequence == -1000 || o.sequence == 1000) }
                if (currentSequence < 0 || currentSequence == 1000) { return (o.sequence == currentSequence) }                
            });
            system.log(["Object Output...", selectionOutput]);
            return selectionOutput;
        }
    });

    //Sunil Start
    // var indexServiceTypeCodes = settings.getServiceTypeCodeIndex();
    var indexServiceTypeCodes = ko.computed(function () {
        if (currentIncident() == null) { return settings.getServiceTypeCodeIndex(); }
        else {
            /*
            var selectionOutputText = settings.getServiceTypeCodeIndex().filter(function (o) {
                var jobcode = incidentsDetails.currentIncident().jobCode();
                // alert(jobcode);
                var d = new Date();
                var curr_hour = d.getHours();
                if (jobcode == "TOWING" || jobcode == "ACCIDENT") {
                    return (o.sequence >= 3)
                } else {
                    if (curr_hour >= 8 && curr_hour <= 20)
                        //String
                    {

                        return (o.sequence == 1)
                    } else {
                        return (o.sequence == 2)
                    }
                }
                

            });
            return selectionOutputText
            */return settings.getServiceTypeCodeIndex();
        }
    });


    //Sunil End

    

    var listOperatorSelection = ko.computed(function () {
        if (currentIncident() == null) { return [{ text: 'No Incident', value: 'NONE' }]; }
        else {            
            var returnArray = [];
            ko.utils.arrayForEach(incidentHubAdapter.connectedUserList(), function (user) { system.log(["Computing User....", user]); returnArray.push({ text: user.name, value: user.providerUserID }); });
            system.log(["Computed Select List", returnArray]);
            return returnArray;
        }
    });

    var listPaymentSelection = ko.observableArray([
            { text: 'Cash Payment', value: 'PAYMENT-CASH' },
            { text: 'Failure to Pay', value: 'PAYMENT-FAIL' }
    ]);

    //Service Request Data Objects
    var incidentStatusRequest = {
        "incidentGUID": ko.observable(),
        "newStatusCode": ko.observable(),
        "delayed": ko.observable(),
        "eta": ko.observable(),
        "serviceFee": ko.observable()
    };

    var incidentDetailsRequest = {
        "incidentGUID": ko.observable(),
        "notes": ko.observable(),
        "concertoCaseID": ko.observable(),
    };

    var incidentPaymentRequest = {
        "incidentGUID": ko.observable(),
        "paymentMethod": ko.observable(),
        "paymentAmount": ko.observable(),
    };

    var incidentOperatorRequest = {
        "incidentGUID": ko.observable(),
        "operatorUserProviderID": ko.observable(),
    };

    var incidentCostingRequest = {
        "incidentGUID": ko.observable(),
        "serviceType": ko.observable(),
        "serviceKilometers": ko.observable(),
        "parkingCosts": ko.observable(),
        "tollCosts": ko.observable(),
        "otherCosts": ko.observable(),
        "offsetDiscount": ko.observable()
    };



    //ViewModel State Behavior  

    var stateAdminLocked = ko.computed(function () {
        if (security.checkRole('DIRECTOR')) { return false;}
        var incident = incidents.currentIncident();
        if (incident == null) { return true; }
        else { if (incident.admin_providerUserID() == security.getUser() || incident.admin_providerUserID() == null) { return false; } else { return true; }; }
    });

    var stateLoading = ko.computed(function () { if (currentIncident()) { return false; } else { return "Current Incident Data Loading or None..."; } });
    var stateDetailsEdit = ko.observable(false);
    var stateStatusEdit = ko.observable(false);
    var statePaymentEdit = ko.observable(false);
    var stateCostingEdit = ko.observable(false);
    var stateOperatorEdit = ko.observable(false);

    var stateDiscountEnable = ko.observable(false);

    currentIncident.subscribe(function (newValue) {
        stateDetailsEdit(false);
        stateStatusEdit(false);
        stateCostingEdit(false);
        statePaymentEdit(false);
        stateOperatorEdit(false);
    });

    var incidentsDetails = {

        displayName: 'Incidents Details',
        activate: activate,
        attached: attached,
        currentIncident: currentIncident,
        indexStatusCode: indexStatusCode,
        indexCurrentStatusCodes: indexCurrentStatusCodes,
        stateLoading: stateLoading,
        stateAdminLocked: stateAdminLocked,
        stateStatusEdit: stateStatusEdit,
        stateDetailsEdit: stateDetailsEdit,
        statePaymentEdit: statePaymentEdit,
        stateOperatorEdit: stateOperatorEdit,
        incidentOperatorRequest: incidentOperatorRequest,
        toggleOperatorEdit: toggleOperatorEdit,
        updateIncidentOperator: updateIncidentOperator,
        deselectCurrent: deselectCurrent,
        toggleStatusEdit: toggleStatusEdit,
        toggleDetailsEdit: toggleDetailsEdit,
        togglePaymentEdit: togglePaymentEdit,
        updateIncidentStatus: updateIncidentStatus,
        updateIncidentPayment: updateIncidentPayment,
        updateIncidentDetails: updateIncidentDetails,
        incidentStatusRequest: incidentStatusRequest,
        incidentDetailsRequest: incidentDetailsRequest,
        incidentPaymentRequest: incidentPaymentRequest,
        listPaymentSelection: listPaymentSelection,
        listOperatorSelection: listOperatorSelection,
        editStatusClose: editStatusClose,
        editStatusForward: editStatusForward,
        editStatusBack: editStatusBack,
        stateCostingEdit: stateCostingEdit,
        incidentCostingRequest: incidentCostingRequest,
        toggleCostingEdit: toggleCostingEdit,
        indexServiceTypeCodes:indexServiceTypeCodes,
        updateIncidentCosting: updateIncidentCosting,
        stateDiscountEnable: stateDiscountEnable,
        triggerCurrentInvoice: triggerCurrentInvoice

    };

    return incidentsDetails;

    //Composition LifeCycle Methods
    function activate() {



    }

    function attached() {


    }

    function editStatusClose() {

        if (incidentsDetails.currentIncident().statusDisplayObj().sequence < 100) { incidentsDetails.incidentStatusRequest.newStatusCode("DECLINED"); }
        else { incidentsDetails.incidentStatusRequest.newStatusCode("FAILED"); }

    }

    function editStatusBack() {

        var baseIndexSequence = incidentsDetails.currentIncident().statusDisplayObj().sequence;

        var backIndexSequence = baseIndexSequence - 1

        if (backIndexSequence < 1) { backIndexSequence = 1; }
        else if (backIndexSequence < 100 && backIndexSequence > 10) { backIndexSequence = 5; }
        else if (backIndexSequence > 100) { backIndexSequence = 100; }

        var lookupIndex = $.grep(incidentsDetails.indexStatusCode, function (e) { return e.sequence == backIndexSequence; });
        incidentsDetails.incidentStatusRequest.newStatusCode(lookupIndex[0].code);

    }

    function editStatusForward() {

        var baseIndexSequence = incidentsDetails.currentIncident().statusDisplayObj().sequence;

        var forwardIndexSequence = baseIndexSequence + 1

        if (forwardIndexSequence > 1000) { forwardIndexSequence = 1000; }
        else if (forwardIndexSequence > 100 && forwardIndexSequence < 1000) { forwardIndexSequence = 1000; }
        else if (forwardIndexSequence < 100 && forwardIndexSequence > 5) { forwardIndexSequence = 100; }

        var lookupIndex = $.grep(incidentsDetails.indexStatusCode, function (e) { return e.sequence == forwardIndexSequence; });
        incidentsDetails.incidentStatusRequest.newStatusCode(lookupIndex[0].code);

    }

    //Bound Methods

    function deselectCurrent() {

        incidents.currentIncident(null);
        incidents.hashIncidentGUID(null);
        router.navigate('#incidents', false);
    }

    function toggleStatusEdit() {

        if (incidentsDetails.stateStatusEdit()) { incidentsDetails.stateStatusEdit(false); }
        else {

            incidentsDetails.incidentStatusRequest.incidentGUID(incidentsDetails.currentIncident().incidentGUID());
            incidentsDetails.incidentStatusRequest.newStatusCode(incidentsDetails.currentIncident().statusCode());
            incidentsDetails.incidentStatusRequest.eta(incidentsDetails.currentIncident().eta());
            incidentsDetails.incidentStatusRequest.serviceFee(incidentsDetails.currentIncident().serviceFee());

            incidentsDetails.stateStatusEdit(true);

        }

    };

    function togglePaymentEdit() {

        if (incidentsDetails.statePaymentEdit()) { incidentsDetails.statePaymentEdit(false); }
        else {

            incidentsDetails.incidentPaymentRequest.incidentGUID(incidentsDetails.currentIncident().incidentGUID());
            incidentsDetails.incidentPaymentRequest.paymentAmount(incidentsDetails.currentIncident().serviceFee() - incidentsDetails.currentIncident().paymentAmount());
            incidentsDetails.incidentPaymentRequest.paymentMethod("PAYMENT-CASH");

            incidentsDetails.statePaymentEdit(true);

        }

    }


    function toggleDetailsEdit() {

        if (incidentsDetails.stateDetailsEdit()) {incidentsDetails.stateDetailsEdit(false); }
        else {

            incidentsDetails.incidentDetailsRequest.incidentGUID(incidentsDetails.currentIncident().incidentGUID());
            incidentsDetails.incidentDetailsRequest.concertoCaseID(incidentsDetails.currentIncident().concertoCaseID());
            incidentsDetails.incidentDetailsRequest.notes(incidentsDetails.currentIncident().staffNotes());

            incidentsDetails.stateDetailsEdit(true);

        }

    };

    function toggleOperatorEdit() {

        if (incidentsDetails.stateOperatorEdit()) { incidentsDetails.stateOperatorEdit(false); }
        else {

            incidentsDetails.incidentOperatorRequest.incidentGUID(incidentsDetails.currentIncident().incidentGUID());
            incidentsDetails.incidentOperatorRequest.operatorUserProviderID(incidentsDetails.currentIncident().admin_providerUserID());

            incidentsDetails.stateOperatorEdit(true);

        }

    };

    function toggleCostingEdit() {

        if (incidentsDetails.stateCostingEdit()) { incidentsDetails.stateCostingEdit(false); }
        else {

            incidentsDetails.incidentCostingRequest.incidentGUID(incidentsDetails.currentIncident().incidentGUID());
            incidentsDetails.incidentCostingRequest.serviceType(incidentsDetails.currentIncident().statusCode());
            incidentsDetails.incidentCostingRequest.serviceKilometers(((incidentsDetails.currentIncident().serviceKilometers() != 0) ? incidentsDetails.currentIncident().serviceKilometers() : null));
            incidentsDetails.incidentCostingRequest.parkingCosts(((incidentsDetails.currentIncident().parkingCosts() != 0) ? incidentsDetails.currentIncident().parkingCosts() : null));
            incidentsDetails.incidentCostingRequest.tollCosts(((incidentsDetails.currentIncident().tollCosts() != 0) ? incidentsDetails.currentIncident().tollCosts() : null));
            incidentsDetails.incidentCostingRequest.otherCosts(((incidentsDetails.currentIncident().otherCosts() != 0) ? incidentsDetails.currentIncident().otherCosts() : null));
            incidentsDetails.incidentCostingRequest.offsetDiscount(((incidentsDetails.currentIncident().offsetDiscount() != 0) ? incidentsDetails.currentIncident().offsetDiscount() : null));


            incidentsDetails.stateCostingEdit(true);

        }

    }

    function updateIncidentOperator(request, data, event) {

        var operatorRequest = _.transform(request, function (result, val, key) { result[key] = val(); });
        system.log(operatorRequest);

        dataContext.submitIncidentUpdate(operatorRequest, "OPERATOR");
        incidentsDetails.stateOperatorEdit(false);

    }

    function updateIncidentStatus(request, data, event) {

        var statusRequest = _.transform(request, function (result, val, key) { result[key] = val(); });
        system.log(["View: Updating Status...", incidentsDetails.incidentStatusRequest]);

        dataContext.submitIncidentUpdate(statusRequest, "STATUS");
        incidentsDetails.stateStatusEdit(false);

    }    

    function updateIncidentDetails(request, data, event) {

        var detailsRequest = _.transform(request, function (result, val, key) { result[key] = val(); });
        system.log(detailsRequest);

        dataContext.submitIncidentUpdate(detailsRequest, "DETAILS");
        incidentsDetails.stateDetailsEdit(false);

    }

    function updateIncidentCosting(request, data, event) {
        
        if (!request.serviceKilometers()) { request.serviceKilometers(0); }
        if (!request.parkingCosts()) { request.parkingCosts(0); }
        if (!request.tollCosts()) { request.tollCosts(0); }
        if (!request.otherCosts()) { request.otherCosts(0); }
        if (!request.offsetDiscount()) { request.offsetDiscount(0); }

        var costingRequest = _.transform(request, function (result, val, key) { result[key] = val(); });      

        system.log(costingRequest);

        dataContext.submitIncidentUpdate(costingRequest, "COSTING");
        incidentsDetails.stateCostingEdit(false);      

    }

    function updateIncidentPayment(paymentStatus, data, event) {

        system.log(["View: Passing crafted IncidentPaymentRequest...", incidentsDetails.incidentPaymentRequest]);

        //var paymentRequest = _.transform(incidentsDetails.incidentPaymentRequest, function (result, val, key) { result[key] = val(); });

        var paymentRequest = {};

        paymentRequest.IncidentGUID = incidentsDetails.incidentPaymentRequest.incidentGUID();
        paymentRequest.PaymentMethod = incidentsDetails.incidentPaymentRequest.paymentMethod();
        paymentRequest.PaymentAmount = incidentsDetails.incidentPaymentRequest.paymentAmount();

        if (paymentRequest.PaymentMethod == 'PAYMENT-FAIL') { paymentRequest.PaymentAmount = -1; }

        dataContext.submitIncidentUpdate(paymentRequest, "PAYMENT");
        incidentsDetails.statePaymentEdit(false);


    }
    function triggerCurrentInvoice() { app.trigger('ServiceHubRequest:TriggerManualInvoice', currentIncident().incidentGUID()); }

});