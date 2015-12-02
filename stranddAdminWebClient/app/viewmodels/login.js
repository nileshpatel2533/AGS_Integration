define(['durandal/system', 'durandal/app', 'roadzen/core-ui', 'roadzen/core-security', 'knockout'], function (system, app, ui, security, ko) {

    var phone = ko.observable();
    var password = ko.observable();
    var errorLogin = ko.observable();
    var progressLogin = ko.observable(false);

    var setRootToShell = function () {
        system.log("Setting Application Root to Shell...");
        app.setRoot('viewmodels/shell', 'entrance');
    };

    return {
        phone: phone,
        password: password,
        errorLogin: errorLogin,
        progressLogin: progressLogin,
        setRootToShell: setRootToShell,

        attemptLogin: function () {
            security.zumoLogin(phone(), password()).done(function (result) {
                system.log("Successfully Logged-In")
                progressLogin(false);
                setRootToShell();           
            }, function (err) {                
                system.log("Error Logging-In");
                system.log(err.message);
                progressLogin(false);
                errorLogin(err.message);                
            }, function (prog) {
                system.log("Logging-In In-Progress...");
                progressLogin(true);
            });
        }
    };
});