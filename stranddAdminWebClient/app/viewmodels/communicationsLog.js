define(['durandal/system', 'plugins/http', 'plugins/router', 'durandal/app', 'app-settings', 'roadzen/core-security', 'knockout'], function (system, http, router, app, settings, security, ko) {
  
    //ViewModel State Behavior
    var stateLoading = ko.observable("Communications Log Loading...");

    //SCM Access Settings
    var url = settings.getCommunicationsLogUrl();
    var qs = {};
    var headers = { "z-xumo-auth": security.getToken() };

    //Main Data
    var logCore = ko.observableArray();
    var timestampUpdate = ko.observable();

    var staff = {

        displayName: 'Communications Log',
        stateLoading: stateLoading,
        logCore: logCore,
        timestampUpdate: timestampUpdate,
        refreshLogCore: refreshLogCore,
        activate: activate,
        attached: attached,

    };

    return staff;

    //Composition LifeCycle Methods
    function activate() { staff.refreshLogCore(); }

    function attached() {


    }

    //Bound Methods

    function refreshLogCore() {

        http.get(url, qs, headers).then(function (response) {

            response.forEach(function (entry) {
                if (!entry.hasOwnProperty('startTime')) { entry.startTime = null; }
                if (!entry.hasOwnProperty('endTime')) { entry.endTime = null; }
            });
            system.log(['Communications Log',response]);
            staff.logCore(response);
            staff.timestampUpdate(Date.now());
            stateLoading(false);

        });

    }



});