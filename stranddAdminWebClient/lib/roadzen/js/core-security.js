define(['durandal/system', 'plugins/http', 'app-settings', 'Q', 'mclient'], function (system, http, settings, Q) {

    //Internal & Protected Variables
    var MobileServiceClient = WindowsAzure.MobileServiceClient;
    system.log(["MClient Settings...", settings.liveDataService(), settings.liveAppKey()])
    var mClient = new MobileServiceClient(settings.liveDataService(), settings.liveAppKey());

    var accountInfo = {};
    var accountRoles = [];

    //Singleton Public Definition & Return
    var coreSecurity = {

        mClient: mClient,
        zumoLogin: zumoLogin,
        zumoLogout: zumoLogout,
        zumoLocalCache: zumoLocalCache,
        zumoLocalCacheCheck: zumoLocalCacheCheck,
        accountInfo: accountInfo,
        accountRoles: accountRoles,
        getUser: getUser,
        getToken: getToken,
        getName: getName,
        getEmail: getEmail,
        checkRole: checkRole

    }
    return coreSecurity;

    function zumoLogin(phone, password) {

        var deferred = Q.defer();

        coreSecurity.mClient.invokeApi("roadzensecurity/login", {
            method: "post",
            body: { "phone": phone, "password": password }
        }).done(function (result) {
            var login = JSON.parse(result.response);
            system.log(["User Determination", login.user]);
            coreSecurity.mClient.currentUser = login.user.userId;
            coreSecurity.mClient.currentUser.mobileServiceAuthenticationToken = login.authenticationToken;
            localStorage.setItem("loggedInUser", login.user.userId);
            localStorage.setItem("zumoAuthToken", login.authenticationToken);
            setAjaxDefaults();
            http.get(settings.getCurrentAccountInfoUrl(), {}, { "z-xumo-auth": login.authenticationToken }).then(function (inforesponse) {
                system.log(['Account Info', inforesponse]);
                coreSecurity.accountInfo = inforesponse;
                http.get(settings.getCurrentAccountRolesUrl(), {}, { "z-xumo-auth": login.authenticationToken }).then(function (roleresponse) {
                    system.log(['Account Roles', roleresponse]);
                    coreSecurity.accountRoles = roleresponse;
                    deferred.resolve(result);
                });
            });
        }, function (err) {
            deferred.reject(err);
        }, function (prog) {
            deferred.notify(prog);
        });

        return deferred.promise;

    }

    function zumoRegistration(phone, name, email, password) {

        var deferred = Q.defer();

        coreSecurity.mClient.invokeApi("roadzensecurity/registration", {
            method: "post",
            body: { "phone": phone, "name": name, "email": email, "password": password }
        }).done(function (result) {
            var login = JSON.parse(result.response);
            setAjaxDefaults();
            system.log(["Registered User", result.response]);
            deferred.resolve(result);
        }, function (err) {
            deferred.reject(err);
        }, function (prog) {
            deferred.notify(prog);
        });

        return deferred.promise;

    }

    //Publicly Exposed Methods

    function zumoLogout() {

        var deferred = Q.defer();

        coreSecurity.mClient.logout();
        localStorage.removeItem("loggedInUser");
        localStorage.removeItem("zumoAuthToken");

        deferred.resolve();

        return deferred.promise;

    }

    function zumoLocalCache() {
        var deferred = Q.defer();
        coreSecurity.mClient.currentUser = localStorage.getItem("loggedInUser");
        coreSecurity.mClient.currentUser.mobileServiceAuthenticationToken = localStorage.getItem("zumoAuthToken");
        setAjaxDefaults();
        http.get(settings.getCurrentAccountInfoUrl(), {}, { "z-xumo-auth": coreSecurity.getToken() }).then(function (inforesponse) {
            system.log(['Account Info', inforesponse]);
            coreSecurity.accountInfo = inforesponse;
            http.get(settings.getCurrentAccountRolesUrl(), {}, { "z-xumo-auth": coreSecurity.getToken() }).then(function (roleresponse) {
                system.log(['Account Roles', roleresponse]);
                coreSecurity.accountRoles = roleresponse;
                system.log("Logged-in User from Cached Token");
                deferred.resolve();
            });
        });
        return deferred.promise;
    }

    function zumoLocalCacheCheck() { if (localStorage.getItem("loggedInUser") && localStorage.getItem("zumoAuthToken")) { return true; } else { return false; } }
    function getUser() { return localStorage.getItem("loggedInUser"); }
    function getToken() { return localStorage.getItem("zumoAuthToken"); }
    function getName() { return coreSecurity.accountInfo.name; }
    function getEmail() { return coreSecurity.accountInfo.email; }

    function checkRole(roleName) {
        var found = false;
        for (var i = 0; i < coreSecurity.accountRoles.length; i++) { if (coreSecurity.accountRoles[i].roleAssignment == roleName) { found = true; break; } }
        return found;
    }
    //Private Methods
    function setAjaxDefaults() { $.ajaxSetup({ beforeSend: function (xhr) { xhr.setRequestHeader('x-zumo-auth', getToken()); } }); }

});

