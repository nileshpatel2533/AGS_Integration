define(['durandal/system', 'durandal/app', 'plugins/http', 'app-settings', 'roadzen/core-security' ,'roadzen/core-ui', 'services/incidentHubAdapter', 'services/notificationHandler', 'breeze', 'knockout'], function (system, app, http, settings, security, ui, incidentHubAdapter, notificationHandler, breeze, ko) {

    //Set Module Scope Variable
    //var self = this;

    //Internal & Protected Variables
    var IncidentCostingUpdateData;
    var entityManagerLive;
    var entityManagerHistory;
    var adapterSubscription;
    var payloadSubscription;
    var operatorCallRequestConfirmSubscription;
    var IncidentCoastingDetail;
    var InboundCallConfirmSubscription;
    var costingslabDetails;
    //Exposed States & App Event Triggers
    var stateInitialized = ko.observable(false);
    stateInitialized.subscribe(function (newValue) { app.trigger('DataContext:IntializedStateChange', newValue); });
    var stateLiveIncidentStore = ko.observable(false);
    stateLiveIncidentStore.subscribe(function (newValue) { app.trigger('DataContext:LiveIncidentStoreStateChange', newValue); });
    stateLiveIncidentStore.extend({ notify: 'always' });
    var stateHistoryIncidentStore = ko.observable(false);
    stateHistoryIncidentStore.subscribe(function (newValue) { app.trigger('DataContext:HistoryIncidentStoreStateChange', newValue); });
    stateHistoryIncidentStore.extend({ notify: 'always' });


    //Singleton Public Definition & Return
    var dataContext = {

        stateInitialized: stateInitialized,
        stateLiveIncidentStore: stateLiveIncidentStore,
        stateHistoryIncidentStore: stateHistoryIncidentStore,
        initialize: initialize,
        queryIncidents: queryIncidents,
        submitIncidentUpdate: submitIncidentUpdate,
        submitCurrentOperatorCallRequestConfirm: submitCurrentOperatorCallRequestConfirm,
        updateCostingSlab: updateCostingSlab,
        submitInboundMessageCallRequestConfirm: submitInboundMessageCallRequestConfirm

    }
    return dataContext;

    //Publicly Exposed Methods
    function initialize() {

        system.log("DataContext Initializing...");
        initEntityManagers();

        //Register Communications Data Submissions
        operatorCallRequestConfirmSubscription = app.on('NotificationHandler:CurrentOperatorCallRequestConfirm').then(function (dataString) { submitCurrentOperatorCallRequestConfirm(dataString) });

        //Register Hub Adapter Live Sync
        adapterSubscription = app.on('IncidentHubAdapter:EnabledStateChange').then(function (state) { if (state) { resetLiveIncidentStore(); resetHistoryIncidentStore(); } else { stateLiveIncidentStore(false); } });

        //Register Service Hub Listeners
        newIncidentCustomerSubscription = app.on('ServiceIncidentHub:ReceivedNewIncidentCustomer').then(function (data) { updateLiveIncidentStore(data); });
        detailsAdminSubscription = app.on('ServiceIncidentHub:ReceivedIncidentDetailsAdmin').then(function (data) { updateLiveIncidentStore(data); });
        statusAdminSubscription = app.on('ServiceIncidentHub:ReceivedIncidentStatusAdmin').then(function (data) { updateLiveIncidentStore(data); });
        statusCustomerCancelSubscription = app.on('ServiceIncidentHub:ReceivedIncidentStatusCustomerCancel').then(function (data) { updateLiveIncidentStore(data); });
        ratingCustomerSubscription = app.on('ServiceIncidentHub:ReceivedIncidentRatingCustomer').then(function (data) { updateLiveIncidentStore(data); });
        newPaymentubscription = app.on('ServiceIncidentHub:ReceivedNewPayment').then(function (data) { updateLiveIncidentStore(data); });
        errorCustomerSubscription = app.on('ServiceIncidentHub:ReceivedIncidentCustomerError').then(function (data) { updateLiveIncidentStore(data); });
        costingAdminSubscription = app.on('ServiceIncidentHub:ReceivedIncidentCostingAdmin').then(function (data) { updateLiveIncidentStore(data); });
        operatorAdminSubscription = app.on('ServiceIncidentHub:ReceivedIncidentOperatorAdmin').then(function (data) { updateLiveIncidentStore(data); });

        smsCustomerSubscription = app.on('NotificationHandler:InboundMessageCallRequestConfirm').then(function (dataString) { submitInboundMessageCallRequestConfirm(dataString) });

        
        incidentHubAdapter.initialize();
        stateInitialized(true);
    }

    function submitCurrentOperatorCallRequestConfirm(communicationGUID) {
        system.log(["Passing Operator Confirm Request...", communicationGUID])
        http.post(settings.getOperatorCallRequestConfirmationUrl(),
            { communicationguid: communicationGUID },
            { "z-xumo-auth": security.getToken() }).then(
     function (response) {
         system.log(["Operator Call Confirm Response...", response]);
         notificationHandler.updateCallRequestNotificationButton(communicationGUID, response.substring(0, 8));

     });
    }

    //piyush
    function submitInboundMessageCallRequestConfirm(communicationGUID) {
        system.log(["Passing Operator Confirm Request...", communicationGUID])
        http.post(settings.getInboundMessageCallRequestConfirmationUrl(),
            { communicationguid: communicationGUID },
            { "z-xumo-auth": security.getToken() }).then(
     function (response) {
         system.log(["Operator Call Confirm Response...", response]);
         //notificationHandler.updateCallRequestNotificationButton(communicationGUID, response.substring(0, 8));

     });
    }

    function queryIncidents(mode, orderQueryCode) {

        //PENDING: To Add a More Dynamic Query Principal Argument Processer

        var selectProjection = "incidentGUID"
        + ", " + "eta"
        + ", " + "staffNotes"
        + ", " + "customerComments"
        + ", " + "accountGUID"
        + ", " + "concertoCaseID"
        + ", " + "coordinateX"
        + ", " + "coordinateY"
        + ", " + "statusCode"
        + ", " + "paymentMethod"
        + ", " + "paymentAmount"
        + ", " + "serviceFee"
        + ", " + "rating"
        + ", " + "concertoCaseID"
        + ", " + "providerArrivalTime"
        + ", " + "createdAt"
        + ", " + "updatedAt"
        + ", " + "jobCode"
        + ", " + "statusCode"
        + ", " + "additionalDetails"
        + ", " + "adminAccountGUID"
        + ", " + "accountGUID"
        + ", " + "vehicleGUID"
        + ", " + "locationKey"
        + ", " + "admin.name"
        + ", " + "admin.phone"
        + ", " + "admin.providerUserID"
        + ", " + "account.name"
        + ", " + "account.phone"
        + ", " + "account.email"
        + ", " + "account.providerUserID"
        + ", " + "vehicle.registrationNumber"
        + ", " + "vehicle.color"
        + ", " + "vehicle.year"
        + ", " + "vehicle.make"
        + ", " + "vehicle.model"
        + ", " + "location.x"
        + ", " + "location.y"
        + ", " + "location.rGDisplay"
        + ", " + "location.landmark"
        + ", " + "location.streetAddress"
        + ", " + "location.city"
        + ", " + "location.state"
        + ", " + "location.zipCode"
        + ", " + "location.country"
        + ", " + "serviceType"
        + ", " + "calculatedBaseServiceCost"
        + ", " + "taxZoneRate"
        + ", " + "serviceKilometers"
        + ", " + "parkingCosts"
        + ", " + "tollCosts"
        + ", " + "otherCosts"
        + ", " + "offsetDiscount"
        + ", " + "calculatedSubtotal"
        + ", " + "calculatedTaxes"
        + ", " + "calculatedTotalCost";

        var query = breeze.EntityQuery
            .from('Incidents');

        if (orderQueryCode) {

            query = query
                .orderBy(orderQueryCode);

        }

        //Return pure JS Objects
        query = query
            .select(selectProjection);

        var results;
        if (mode == "active") { results = entityManagerLive.executeQueryLocally(query); }
        else if (mode == "inactive") { results = entityManagerHistory.executeQueryLocally(query); }

        return results;  // query the cache (synchronous)

    }

    //Private Internal Functions



    function initEntityManagers() {

        // Configure our DataService that we will use [Essentially Avoided in lieu of custom SignalR Adapters
        var dataServiceDefinition = new breeze.DataService({

            serviceName: settings.liveDataService(),
            hasServerMetadata: false

        });

        //Configure our MetaDataStore
        var metadataStoreDefinition = new breeze.MetadataStore();
        require(['datamodels/entityIncident'], function (model) { metadataStoreDefinition.addEntityType(model.entityDefinition); metadataStoreDefinition.registerEntityTypeCtor('Incident', null, model.entityConstructor); });
        require(['datamodels/entityLocation'], function (model) { metadataStoreDefinition.addEntityType(model.entityDefinition); });
        require(['datamodels/entityAccount'], function (model) { metadataStoreDefinition.addEntityType(model.entityDefinition); });
        require(['datamodels/entityVehicle'], function (model) { metadataStoreDefinition.addEntityType(model.entityDefinition); });

        //Set the manager from Definitions
        entityManagerLive = new breeze.EntityManager({ dataService: dataServiceDefinition, metadataStore: metadataStoreDefinition });
        entityManagerHistory = entityManagerLive.createEmptyCopy();
    }

    function resetLiveIncidentStore() { incidentHubAdapter.loadActiveServiceData(entityManagerLive).done(function () { stateLiveIncidentStore(true); }, function (err) { stateLiveIncidentStore(false); system.log(err); }); }
    function resetHistoryIncidentStore() { incidentHubAdapter.loadInactiveServiceData(entityManagerHistory).done(function () { stateHistoryIncidentStore(true); }, function (err) { stateHistoryIncidentStore(false); system.log(err); }); }

    function updateLiveIncidentStore(data) {

        incidentHubAdapter.processIncomingData(entityManagerLive, data);
        stateLiveIncidentStore(true);
        system.log(["Incident Store Updated", data]);

    }

    function updateCostingSlab(request, requestType) {
        incidentHubAdapter.processServiceRequest(request, requestType);
    }

    function submitIncidentUpdate(request, requestType) { incidentHubAdapter.processServiceRequest(request, requestType); }


});