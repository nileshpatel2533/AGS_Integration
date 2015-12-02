define(['durandal/system', 'durandal/app', 'roadzen/core-security', 'services/incidentHubAdapter', 'app-settings', 'Q', 'knockout', 'jquery.signalR'], function (system, app, security, incidentHubAdapter, settings, Q, ko) {

    //Exposed States & App Event Triggers
    var stateInitialized = ko.observable(false);
    stateInitialized.subscribe(function (newValue) {app.trigger('LiveDataConnection:IntializedStateChange', newValue);});

    var stateConnected = ko.observable(false);
    stateConnected.subscribe(function (newValue) {
        app.trigger('LiveDataConnection:ConnectedStateChange', newValue);
    });

    //Singleton Public Definition & Return
    var liveDataConnection = {

        stateInitialized : stateInitialized,
        stateConnected : stateConnected,
        initialize: initialize

    }
    return liveDataConnection;

    //Publicly Exposed Methods
    function initialize() {

        require(['services/serverHubProxy'], function (){

            // Designate SignalR Connection URL
            $.connection.hub.url = settings.getSignalRConnectionUrl();
            
            incidentHubAdapter.registerClientFunctions();

            // Enforce Long Polling Transport... [Consider AMS QS Authorization & Web Socket Protocol Upgrades; or using Basic for WS]
            //Start Connection and Indicate Success
            $.connection.hub.start({ transport: 'longPolling' }).done(function () { stateConnected(true); }).fail(function (err) { stateConnected(false);
                system.log(err); });

            $.connection.hub.disconnected( function (){ stateConnected(false); system.log('SignalR Connection Disconnected'); });

        });
        
        stateInitialized(true);

    }
});

