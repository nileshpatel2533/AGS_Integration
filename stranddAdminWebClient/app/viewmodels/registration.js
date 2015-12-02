define(['durandal/system', 'durandal/app', 'roadzen/core-ui', 'roadzen/core-security', 'knockout'], function (system, app, ui, security, ko) {

    var phone = ko.observable();
    var password = ko.observable();
    var errorRegistration = ko.observable();
    var progressRegistration = ko.observable(false);

    var setRootToShell = function () {
        system.log("Setting Application Root to Shell...");
        app.setRoot('viewmodels/shell', 'entrance');
    };

    return {
        phone: phone,
        password: password,
        errorRegistration: errorRegistration,
        progressRegistration: progressRegistration,
        setRootToShell: setRootToShell,

        attemptRegistration: function () {
            security.zumoRegistration(phone(), name(), email(), password()).done(function (result) {
                system.log("Successfully Registered")
                progressRegistration(false);
                setRootToShell();           
            }, function (err) {                
                system.log("Error Registering");
                system.log(err.message);
                progressRegistration(false);
                errorRegistration(err.message);                
            }, function (prog) {
                system.log("Registration In-Progress...");
                progressRegistration(true);
            });
        }
    };
});