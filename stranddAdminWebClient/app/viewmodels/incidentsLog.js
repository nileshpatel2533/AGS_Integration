define(['durandal/system', 'plugins/router', 'durandal/app', 'app-settings', 'roadzen/core-security', 'roadzen/core-ui', 'services/dataContext', 'viewmodels/incidents', 'knockout', 'viewmodels/incidentsDetails'], function (system, router, app, settings, security, ui, dataContext, incidents, ko, incidentsDetails) {

    //ViewModel State Behavior
    var stateLoading = ko.observable("Log Data Loading or None...");

    // Constructor for an object with two properties
    var Selection = function (text, value) {
        this.selectionText = text;
        this.selectionValue = value;
    };


    var urlExcel = ko.observable(settings.getIncidentsExcelUrl());
    var titleExcel = ko.observable("Generate Excel");

    // Selection 
    var listLogSelection = ko.observableArray([
            { text: 'Active Incidents', value: true },
    ])

    historyStoreSubscription = app.on('DataContext:HistoryIncidentStoreStateChange').then(function (state) { if (state && listLogSelection.indexOf({ text: 'All Inactive Incidents', value: false }) < 0) { listLogSelection.push({ text: 'All Inactive Incidents', value: false }); } });

    var activeLogIndicator = {
        class: ko.observable("label label-warning"),
        display: ko.observable("<strong><i class='fa fa-circle-o-notch fa-spin'></i> Active Incident Loading In Progress</strong>")
    };

    var historyLogIndicator = {
        class: ko.observable("label label-warning"),
        display: ko.observable("<strong><i class='fa fa-circle-o-notch fa-spin'></i> Inactive Incident Loading In Progress</strong>")
    };

    app.on('DataContext:LiveIncidentStoreStateChange').then(function (state) { if (state) { activeLogIndicator.class('label label-success'); activeLogIndicator.display('<strong><i class="fa fa-check"></i> Active Incident Loading Complete</strong>'); } else { activeLogIndicator.class('label label-warning'); activeLogIndicator.display('<strong><i class="fa fa-circle-o-notch fa-spin"></i> Active Incident Loading In Progress</strong>'); } });
    app.on('DataContext:HistoryIncidentStoreStateChange').then(function (state) { if (state) { historyLogIndicator.class('label label-success'); historyLogIndicator.display('<strong><i class="fa fa-check"></i> Inactive Incident Loading Complete</strong>'); } else { historyLogIndicator.class('label label-warning'); historyLogIndicator.display('<strong><i class="fa fa-circle-o-notch fa-spin"></i> Inactive Incident Loading In Progress</strong>'); } });


    var currentLogSelection = ko.observable

    var incidentsLog = {

        displayName: 'Incidents Log',
        stateLoading: stateLoading,
        activate: activate,
        attached: attached,
        selectIncident: selectIncident,
        listLogSelection: listLogSelection,
        currentLogSelection: currentLogSelection,
        activeLogIndicator: activeLogIndicator,
        historyLogIndicator: historyLogIndicator

    };

    return incidentsLog;

    //Composition LifeCycle Methods
    function activate() {


    }

    function attached() {


    }

    //Bound Methods

    function selectIncident(incident) {

        incidents.currentIncident(incident);
        incidents.hashIncidentGUID(incident.incidentGUID());
        ui.smoothScrollTop();
        incidentsDetails.indexServiceTypeCodes();
        router.navigate('#incidents/' + incident.incidentGUID(), false);
        
    };

});