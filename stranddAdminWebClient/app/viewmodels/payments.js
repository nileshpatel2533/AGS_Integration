define(['durandal/system', 'plugins/http', 'durandal/app', 'app-settings', 'roadzen/core-security', 'knockout'], function (system, http, app, settings, security, ko) {

    //SCM Access Settings
    var url = settings.getPaymentsUrl();
    var qs = {};
    var headers = { "z-xumo-auth": security.getToken() };

    //Main Data
    var logCore = ko.observableArray();
    var timestampUpdate = ko.observable();

    //ViewModel State Behavior
    var stateLoading = ko.observable("Loading Payments Log...");

    var payments = {

        displayName: 'Payments Log',
        stateLoading: stateLoading,
        logCore: logCore,
        timestampUpdate: timestampUpdate,
        refreshLogCore: refreshLogCore,
        activate: activate,

    };

    return payments;

    //Composition LifeCycle Methods
    function activate() { payments.refreshLogCore(); }

    //Bound Methods

    function refreshLogCore() {

        http.get(url, qs, headers).then(function (response) {

            
            response.forEach(function (entry) { if (!entry.hasOwnProperty('incidentGUID')) { entry.incidentGUID = null; } });
            system.log(response);
            payments.logCore(response);
            logCore.reverse();
            payments.timestampUpdate(Date.now());
            stateLoading(false);

        });

    }


});