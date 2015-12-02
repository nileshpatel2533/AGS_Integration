define(['durandal/system', 'durandal/app', 'app-settings', 'services/liveDataConnection', 'Q', 'breeze', 'knockout', 'lodash'], function (system, app, settings, liveDataConnection, Q, breeze, ko, _) {

    // Declare a proxy to reference the hub. 
    var hubProxy = function () { return $.connection.incidentHub }; // the generated client-side hub proxy

    //Hub Definition

    var connectedUserList = ko.observableArray();

    //Exposed States & App Event Triggers
    var stateInitialized = ko.observable(false);
    stateInitialized.subscribe(function (newValue) {
        app.trigger('IncidentHubAdapter:IntializedStateChange', newValue);
    });
    var stateClientFunctionsRegistered = ko.observable(false);
    stateClientFunctionsRegistered.subscribe(function (newValue) {
        app.trigger('IncidentHubAdapter:ClientFunctionsRegisteredStateChange', newValue);
    });
    var stateEnabled = ko.observable(false);
    stateEnabled.subscribe(function (newValue) {
        app.trigger('IncidentHubAdapter:EnabledStateChange', newValue);
    });

    //Singleton Public Definition & Return  
    incidentHubAdapter = {

        hubProxy: hubProxy,
        stateInitialized: stateInitialized,
        stateClientFunctionsRegistered: stateClientFunctionsRegistered,
        stateEnabled: stateEnabled,
        initialize: initialize,
        registerClientFunctions: registerClientFunctions,
        loadActiveServiceData: loadActiveServiceData,
        loadInactiveServiceData: loadInactiveServiceData,
        processIncomingData: processIncomingData,
        processServiceRequest: processServiceRequest,
        connectedUserList: connectedUserList
    }
    return incidentHubAdapter;


    //Publicly Exposed Methods
    function initialize() {

        system.log("IncidentHubAdapater Initializing...");

        app.on('LiveDataConnection:ConnectedStateChange').then(function (state) {
            if (state) {
                incidentHubAdapter.stateEnabled(true);
                getConnectedUserList();
                system.log(["Connected User List Processed", incidentHubAdapter.connectedUserList()]);
            } else { incidentHubAdapter.stateEnabled(false); }
        });

        manualInvoiceSubscription = app.on('ServiceHubRequest:TriggerManualInvoice').then(function (incidentGUID) { hubProxy().server.sendIncidentCurrentInvoice(incidentGUID).done(function (responseString) { app.trigger('IncidentHubAdapter:ManualInvoiceTriggerSubmitted', responseString); }); });

        incidentHubAdapter.stateInitialized(true);

    }
    
    function loadActiveServiceData(em) {
        var deferred = Q.defer();

        if (incidentHubAdapter.stateEnabled()) {

            //Call Service-Side Hub Method to Retreive All InitialData and Store into Entity Manager            
            hubProxy().server.getActiveIncidents().done(function (allData) {

                system.log(["Initial Full Payload received from StrandD Service Incident Hub", allData]);
                allData.forEach(function (data) { saveAsLocalEntities(em, data); });
                deferred.resolve();

            }).fail(function (err) { system.log(["Error receiving Full Payload from StrandD Service Incident Hub", err]); deferred.reject(err); });

        }
        else {

            var err = "Incident Hub Not Currently Enabled";
            system.log(err);
            deferred.reject(err);
        }
        return deferred.promise;
    }

    function loadInactiveServiceData(em) {
        var deferred = Q.defer();

        if (incidentHubAdapter.stateEnabled()) {

            //Call Service-Side Hub Method to Retreive All InitialData and Store into Entity Manager
            //Currently Pulling from Configured Timespan Lapse
            hubProxy().server.getInactiveIncidents(settings.defaultHistoryHours).done(function (allData) {

                system.log(["Inactive Incident Full Payload received from StrandD Service Incident Hub", allData]);
                allData.forEach(function (data) { saveAsLocalEntities(em, data); });
                deferred.resolve();

            }).fail(function (err) { system.log(["Error receiving Inactive Incident Full Payload from StrandD Service Incident Hub", err]); deferred.reject(err); });

        }
        else {

            var err = "Incident Hub Not Currently Enabled";
            system.log(err);
            deferred.reject(err);
        }
        return deferred.promise;
    }

    function processIncomingData(em, data) {

        system.log(["Processing Incoming Data...", data]);
        
        if (data.actionType == "NEW") { saveAsLocalEntities(em, data); }
        else if (data.actionType == "UPDATE") { updateLocalEntities(em, data); }
        else if (data.actionType == "UPDATE_FROMPAYMENT") {

            incidentData = { actionType: data.actionType, incidentGUID: data.IncidentGUID, paymentAmount: data.Amount, paymentMethod: data.PaymentPlatform };
            updateLocalEntities(em, incidentData);

        }

    }

    function processServiceRequest(request, requestType) {
        if (requestType == "STATUS") { hubProxy().server.updateStatus(request).done(function (responseString) { app.trigger('IncidentHubAdapter:StatusUpdateSubmitted', responseString); }); }
        if (requestType == "DETAILS") { hubProxy().server.updateDetails(request).done(function (responseString) { app.trigger('IncidentHubAdapter:DetailsUpdateSubmitted', responseString); }); }
        if (requestType == "PAYMENT") { hubProxy().server.updatePayment(request).done(function (responseString) { app.trigger('IncidentHubAdapter:PaymentUpdateSubmitted', responseString); }); }
        if (requestType == "COSTING") { hubProxy().server.updateCosting(request).done(function (responseString) { app.trigger('IncidentHubAdapter:CostingUpdateSubmitted', responseString); }); }
        if (requestType == "OPERATOR") { hubProxy().server.assignIncidentOperator(request).done(function (responseString) { app.trigger('IncidentHubAdapter:OperatorUpdateSubmitted', responseString); }); }
    }

    function registerClientFunctions() {

        //Register Hub Client Functions

        hubProxy().client.saveNewIncidentCustomer = function (data) { data.actionType = "NEW"; app.trigger('ServiceIncidentHub:ReceivedNewIncidentCustomer', data); };
        hubProxy().client.updateIncidentOperatorAdmin = function (data) { data.actionType = "UPDATE"; app.trigger('ServiceIncidentHub:ReceivedIncidentOperatorAdmin', data); };
        hubProxy().client.updateIncidentDetailsAdmin = function (data) { data.actionType = "UPDATE"; app.trigger('ServiceIncidentHub:ReceivedIncidentDetailsAdmin', data); };
        hubProxy().client.updateIncidentStatusAdmin = function (data) { data.actionType = "UPDATE"; app.trigger('ServiceIncidentHub:ReceivedIncidentStatusAdmin', data); };
        hubProxy().client.updateIncidentCustomerError = function (data) { data.actionType = "UPDATE"; app.trigger('ServiceIncidentHub:ReceivedIncidentCustomerError', data); };
        hubProxy().client.updateIncidentStatusCustomerCancel = function (data) { data.actionType = "UPDATE"; app.trigger('ServiceIncidentHub:ReceivedIncidentStatusCustomerCancel', data); };
        hubProxy().client.updateIncidentRatingCustomer = function (data) { data.actionType = "UPDATE"; app.trigger('ServiceIncidentHub:ReceivedIncidentRatingCustomer', data); };
        hubProxy().client.saveNewPayment = function (data) { data.actionType = "UPDATE_FROMPAYMENT"; app.trigger('ServiceIncidentHub:ReceivedNewPayment', data); };
        hubProxy().client.notifyCustomerClientExceptionContact = function (data) { app.trigger('ServiceIncidentHub:ReceivedNewCustomerClientExceptionContact', data); };
        hubProxy().client.updateIncidentCostingAdmin = function (data) { data.actionType = "UPDATE"; app.trigger('ServiceIncidentHub:ReceivedIncidentCostingAdmin', data); };

        hubProxy().client.notifyCustomerNewInboundSmsContact = function (data) { app.trigger('ServiceIncidentHub:ReceivedNewInboundSms', data); };

        incidentHubAdapter.stateClientFunctionsRegistered(true);

    }

    //Private Internal Functions

    function getConnectedUserList() {
        hubProxy().server.getConnectedUserList().done(function (data) {
            system.log(["Connected User List Retrieved", data]);
            for (var key in data) {
                if (data.hasOwnProperty(key)) {
                    objUser = data[key];
                    objListUser = _.transform(objUser, function (result, val, key) { result[key.lcFirstLetter()] = val; });
                    incidentHubAdapter.connectedUserList.push(objListUser);
                }
            }
        });
    }

    function saveAsLocalEntities(em, data) {

        var incidentValues = _.transform(data, function (result, val, key) {
            result[key.lcFirstLetter()] = val;
        });

        var incidentEntityType = em.metadataStore.getEntityType('Incident');
        var locationEntityType = em.metadataStore.getEntityType('Location');

        //Pull The Location Object and Create Entity
        if ('locationObj' in incidentValues) {

            var locationValues = _.transform(incidentValues.locationObj, function (result, val, key) { result[key.lcFirstLetter()] = val; });

            locationValues.locationKey = incidentValues.incidentGUID;
            var newLocation = locationEntityType.createEntity(locationValues);
            em.addEntity(newLocation, breeze.EntityState.Unchanged, breeze.MergeStrategy.SkipMerge);
            incidentValues.locationKey = incidentValues.incidentGUID;
        }

        if ('incidentUserInfo' in incidentValues) {

            var accountValues = _.transform(incidentValues.incidentUserInfo, function (result, val, key) {
                result[key.lcFirstLetter()] = val;
            });

            if (accountValues.accountGUID != 'NO ASSOCIATED USER') {

                if (accountValues.accountGUID == 'NO USER ACCOUNT') { accountValues.accountGUID = accountValues.providerUserID }
                newAccount = em.createEntity('Account', accountValues, breeze.EntityState.Unchanged, breeze.MergeStrategy.SkipMerge);
                incidentValues.accountGUID = newAccount.getProperty('accountGUID');

            }

        }

        if ('confirmedAdminAccount' in incidentValues) {

            var accountValues = _.transform(incidentValues.confirmedAdminAccount, function (result, val, key) {
                result[key.lcFirstLetter()] = val;
            });

            if (accountValues.accountGUID != 'NO ASSOCIATED USER') {

                if (accountValues.accountGUID == 'NO USER ACCOUNT') { accountValues.accountGUID = accountValues.providerUserID }
                newAccount = em.createEntity('Account', accountValues, breeze.EntityState.Unchanged, breeze.MergeStrategy.SkipMerge);
                incidentValues.adminAccountGUID = newAccount.getProperty('accountGUID');

            }

        }

        if ('incidentVehicleInfo' in incidentValues) {

            var vehicleValues = _.transform(incidentValues.incidentVehicleInfo, function (result, val, key) {
                result[key.lcFirstLetter()] = val;
            });

            if (vehicleValues.vehicleGUID != 'NO ASSOCIATED VEHICLE') {

                newVehicle = em.createEntity('Vehicle', vehicleValues, breeze.EntityState.Unchanged, breeze.MergeStrategy.SkipMerge);
                incidentValues.vehicleGUID = newVehicle.getProperty('vehicleGUID');

            }

        }

        //Create the Incident Entity
        newIncident = em.createEntity(incidentEntityType, incidentValues, breeze.EntityState.Unchanged, breeze.MergeStrategy.OverwriteChanges);

    }

    function updateLocalEntities(em, data) {
        
        if (data.actionType == "UPDATE") {

            if ('ConfirmedAdminAccount' in data) {

                var accountValues = _.transform(data.ConfirmedAdminAccount, function (result, val, key) {
                    result[key.lcFirstLetter()] = val;
                });

                if (accountValues.accountGUID != 'NO ASSOCIATED USER') {

                    if (accountValues.accountGUID == 'NO USER ACCOUNT') { accountValues.accountGUID = accountValues.providerUserID }
                    newAccount = em.createEntity('Account', accountValues, breeze.EntityState.Unchanged, breeze.MergeStrategy.SkipMerge);
                    data.adminAccountGUID = newAccount.getProperty('accountGUID');

                }
            }

            delete data.actionType;
            delete data.LocationObj;
            delete data.ConfirmedAdminAccount;
            delete data.IncidentUserInfo;
            delete data.IncidentVehicleInfo;
            var incidentValues = _.transform(data, function (result, val, key) { result[key.lcFirstLetter()] = val; });
            
            var incidentEntity = em.getEntityByKey("Incident", incidentValues.incidentGUID);
            for (var property in incidentValues) { if (incidentValues.hasOwnProperty(property)) { if (incidentValues[property]) { incidentEntity.setProperty(property, incidentValues[property]); } } }

        }
        else if (data.actionType == "UPDATE_FROMPAYMENT") {
            var incidentPaymentValues = data;
            var incidentEntity = em.getEntityByKey("Incident", incidentPaymentValues.incidentGUID);

            var currentPaymentAmount = incidentEntity.getProperty('paymentAmount'); // (incidentEntity.getProperty('paymentAmount')) ? incidentEntity.getProperty('paymentAmount') : 0;
            var currentPaymentMethod = incidentEntity.getProperty('paymentMethod');

            system.log(["Incoming Incident Payment Values", incidentPaymentValues])
            system.log(["Prior & New Current Payment Amount", currentPaymentAmount, incidentPaymentValues.paymentAmount]);
            var newPaymentAmount = (parseFloat(currentPaymentAmount) + parseFloat(incidentPaymentValues.paymentAmount));
            system.log(["New Full Payment Amount", newPaymentAmount]);

            incidentEntity.setProperty('paymentAmount', newPaymentAmount);
            if (!currentPaymentMethod) { incidentEntity.setProperty('paymentMethod', (incidentPaymentValues.paymentMethod)); }
            else { incidentEntity.setProperty('paymentMethod', 'Multiple Payments'); }
        }

        

    }

});
