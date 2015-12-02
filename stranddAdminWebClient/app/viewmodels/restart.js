define(['durandal/system', 'durandal/app', 'roadzen/core-ui', 'roadzen/core-security', 'knockout'], function (system, app, ui, security, ko) {

    var stateLogout = ko.observable("The Application needs Restarted");
    var attemptRestart = function () {
        system.log("Restarting Application...");
        window.location.reload();
    };

    return {
        stateLogout: stateLogout,
        attemptRestart: attemptRestart
    };
});