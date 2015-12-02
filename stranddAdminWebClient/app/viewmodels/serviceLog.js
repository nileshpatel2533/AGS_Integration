define(['durandal/system', 'plugins/http', 'durandal/app', 'app-settings', 'roadzen/core-security', 'knockout'], function (system, http, app, settings, security, ko) {

    //SCM Access Settings
    var url = settings.getSCMLogsUrl();
    var qs = {};
    var headers = { "z-xumo-auth": security.getToken() };

    //Main Data
    var logCore = ko.observableArray();
    var timestampUpdate = ko.observable();

    //ViewModel State Behavior
    var stateLoading = ko.observable("Loading Logs from Azure SCM...");

    var serviceLog = {

        displayName: 'Service Log',
        stateLoading: stateLoading,
        logCore: logCore,
        timestampUpdate: timestampUpdate,
        refreshLogCore: refreshLogCore,
        activate: activate,

    };

    return serviceLog;

    //Composition LifeCycle Methods
    function activate() { serviceLog.refreshLogCore(); }

    //Bound Methods

    function refreshLogCore() {

        http.get(url, qs, headers).then(function (response) {

            returnLog = JSON.parse(response);

            returnLog.forEach(function (entry) {
                //Modify returned SCM Message String into JSON String and Parse for Details Object

                var parseFlag = true;

                if (parseFlag) {

                    if (entry.message.indexOf("Exception=") > -1) {
                        entry.message = entry.message.replace("Exception=", '"exception":"');
                        entry.message = entry.message.replace(", Id=", '","id":"');
                    }
                    else {
                        entry.message = entry.message.replace("Message='", '"message":"');
                        entry.message = entry.message.replace("', Id=", '","id":"');
                    }

                    entry.message = entry.message.replace(", Category='", '","category":"');
                    entry.message = entry.message.slice(0, -1);
                    entry.message = '{' + entry.message + '"}';
                    system.log(["Parsing Service Log Entry Message...", entry.message]);
                    entry.details = JSON.parse(entry.message);

                    if (!entry.details.hasOwnProperty("message")) { entry.details.message = entry.message; }
                    if (!entry.details.hasOwnProperty("category")) { entry.details.category = ""; }

                }
                else {
                    entry.details.message = entry.message;
                    entry.details.category = "";
                }
            });
            serviceLog.logCore(returnLog);
            serviceLog.timestampUpdate(Date.now());
            stateLoading(false);

        });

    }


});