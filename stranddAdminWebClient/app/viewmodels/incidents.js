define(['durandal/system', 'plugins/router', 'durandal/app', 'roadzen/core-ui', 'services/dataContext', 'datamodels/entityIncident', 'knockout'], function (system, router, app, ui, dataContext, entityIncident, ko) {

    //Router Stream
    var hashIncidentGUID = ko.observable();

    //View Model Behaviorials
    var sectionMap = ko.observable("viewmodels/incidentsMap");
    var sectionLog = ko.observable("viewmodels/incidentsLog");
    var sectionDetails = ko.computed(function () { if (hashIncidentGUID()) { return "viewmodels/incidentsDetails"; } else { return "viewmodels/incidentsHighlights"; } });

    var stateMapInit = ko.observable("Initializing Map...");
    var stateLogInit = ko.observable("Initializing Log...");
    var stateDetailsInit = ko.computed(function () { if (hashIncidentGUID()) { return "Initializing Details..."; } else { return "Initializing Highlights..."; } });


    //Main View Datas

    var logMode = ko.observable("active");
    var stateLogActive = ko.computed({
        read: function () { return (logMode()=="active"); },
        write: function (value) { if (value) { logMode("active"); } else { logMode("inactive");  } },
        owner: incidents
    });
    stateLogActive.subscribe(function (newValue) { refreshViewData(incidents.logMode()); });

    var log = ko.observableArray([]);
    log.subscribe(function (newValue) { app.trigger('Incidents:LogModified', newValue); });

    var currentIncident = ko.observable();
    currentIncident.subscribe(function (newValue) { app.trigger('Incidents:CurrentIncidentChange', newValue); });

    //Exposed States & App Event Triggers
    var currentQuery = ko.observable(false);
    currentQuery.subscribe(function (newValue) { app.trigger('Incidents:QueryChange', newValue); });

    var incidents = {

        displayName: 'Incidents',
        activate: activate,
        hashIncidentGUID: hashIncidentGUID,
        sectionMap: sectionMap,
        sectionLog: sectionLog,
        sectionDetails: sectionDetails,
        log: log,
        logMode: logMode,
        stateLogActive: stateLogActive,
        currentQuery: currentQuery,
        currentIncident: currentIncident,
        navigateIncident: navigateIncident,
        changeCurrentIncident: changeCurrentIncident

    };

    return incidents;

    //Composition LifeCycle Methods
    function activate(incidentGUID, queryStringParams) {

        incidents.currentIncident(null);

        require(['viewmodels/shell'], function (shell) { if (shell.viewSidebar() == "shellSidebar.html") { shell.toggleSidebar(); } });

        if (incidentGUID) {
            if (!incidentGUID.hasOwnProperty('mode')) { hashIncidentGUID(incidentGUID); }
            else { hashIncidentGUID(null); queryStringParams = incidentGUID; }
        } else { hashIncidentGUID(null); }

        system.log(["Determined Query String Params...", queryStringParams]);
        if (queryStringParams) { if (queryStringParams.hasOwnProperty('mode')) { incidents.logMode(queryStringParams.mode); } }

        //Set Reverse Chronological for Initial View Query
        incidents.currentQuery("createdAt desc");

        refreshViewData(incidents.logMode());
        registerStoreSync();

    }

    //Private Methods

    function injectViewProperties(incidentMember) {

        //system.log(['Injecting View Propety...',incidentMember]);
        incidentMember.isCurrent = ko.computed(function () {

            if (incidents.currentIncident()) { return incidents.currentIncident().incidentGUID() === incidentMember.incidentGUID(); }
            else { return false; }


        });

        return incidentMember;

    }

    function registerStoreSync() {

        app.on('DataContext:LiveIncidentStoreStateChange').then(function (state) {

            if (state) { if (incidents.logMode() == "active") { refreshViewData(incidents.logMode()); } }
            else { if (incidents.logMode() == "active") { incidents.log([]); incidents.currentIncident({}); } }

        });

    }

    function refreshViewData(mode) {

        var storeResults = dataContext.queryIncidents(mode, incidents.currentQuery());

        incidents.log(ko.utils.arrayMap(storeResults, function (incident) {

            //system.log(['Preparing Incident...', incident]);
            var preparedIncident = _.transform(incident, function (result, val, key) { result[key] = ko.observable(val); });
            //system.log(['Preparing Incident, Post Transform...', preparedIncident]);
            preparedIncident = entityIncident.augmentDisplays(preparedIncident);
            //system.log(['Preparing Incident, Post Augment...', preparedIncident]);
            preparedIncident = injectViewProperties(preparedIncident);

            //Check if Incident is the one that is currently meant to be selected (from the Hash/URL)
            if (preparedIncident.incidentGUID() === incidents.hashIncidentGUID()) { incidents.currentIncident(preparedIncident); }

            return preparedIncident;

        }));

        ui.resizeScrollbars();


    }

    function navigateIncident(incidentGUID) {

        system.log(["Incident GUID Passing -navigateIncident", incidentGUID]);
        if (router.activeInstruction().config.title == "Incidents") {

            incidents.hashIncidentGUID(incidentGUID);
            incidents.changeCurrentIncident(incidentGUID);
            router.navigate('#incidents/' + incidentGUID, false);

        }
        else { router.navigate(('#incidents/' + incidentGUID)); }

    }

    function changeCurrentIncident(incidentGUID) {

        var change = ko.utils.arrayFirst(incidents.log(), function (incident) { return incident.incidentGUID() === incidentGUID; });
        currentIncident(change);

    }


});