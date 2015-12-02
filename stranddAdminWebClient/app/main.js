

requirejs.config({
    urlArgs: window.appVersion + "-" + window.buildIteration,
    waitSeconds: 0,
    paths: {

        //App Core [Durandal]
        'async': '../lib/require/plugins/async',
        'text': '../lib/require/text',
        'durandal': '../lib/durandal/js',
        'plugins': '../lib/durandal/js/plugins',
        'transitions': '../lib/durandal/js/transitions',

        //Databinding [Knockout]
        'ko': '../lib/knockout/knockout-3.3.0.debug',

        //Rich Data [Breeze]
        'breeze': '../lib/breeze/breeze.debug',
        'Q': '../lib/breeze/q',

        //Browser [jQuery and Plugins]
        'jquery': '../lib/jquery/jquery-2.1.3',
        'slidebars': '../lib/slidebars/js/slidebars',
        'jquery.dcjqaccordion': '../lib/jquery/plugins/jquery.dcjqaccordion.2.7',
        'jquery.nicescroll': '../lib/jquery/plugins/jquery.nicescroll',
        'jquery.fileDownload': '../lib/jquery/plugins/jquery.fileDownload',
        'jquery.scrollTo': '../lib/jquery/plugins/jquery.scrollTo.min',

        //Real-Time Connection [SignalR]
        'jquery.signalR': '../lib/jquery/plugins/jquery.signalR-2.1.1',

        //AMS Service [Zumo-Client]
        'mclient': '../lib/gmaps/gmaps',

        //GMaps
        'mclient': '../lib/mclient/MobileServices.Web-1.2.5.min',

        //Additional UI & Misc
        'bootstrap': '../lib/bootstrap/js/bootstrap',
        'bootstrap-datetimepicker': '../lib/bootstrap-datetimepicker/js/bootstrap-datetimepicker',
        'gritter': '../lib/gritter/js/jquery.gritter',
        'toastr': '../lib/toastr/js/toastr',
        'moment': '../lib/moment/moment',
        'lodash': '../lib/lodash/lodash',

        //Modular App & RoadZen Library Custom Logic Folder
        'roadzen': '../lib/roadzen/js'

    },
    map: {
        '*': { 'knockout': 'ko' }
    },
    shim: {
        'bootstrap': {
            deps: ['jquery'],
            exports: 'jQuery'
        }
    }
});

define(['durandal/system', 'durandal/app', 'durandal/viewLocator', 'roadzen/core-security', 'roadzen/koBindings', 'services/gmapManager', 'Q'], function (system, app, viewLocator, security, bindings, gmapManager, Q) {

    system.debug(window.debugFlag);
    //if (window.debugFlag) { system.log(["Debug Flag Enabled", window.debugFlag]);}

    app.title = 'StrandD Admin Web Client';

    app.configurePlugins({

        router: true,
        dialog: true

    });

    //Register App-Wide Messaging Logging
    app.on('all').then(function (event) { system.log(arguments); });

    //Utilizing Q for Promises
    system.defer = function (action) {

        var deferred = Q.defer();
        action.call(deferred, deferred);
        var promise = deferred.promise;
        deferred.promise = function () { return promise; };
        return deferred;

    };

    //Set RoadZen Custom KO Bindings
    bindings.registerBase();
    gmapManager.registerMapBindings();

    app.start().then(function () {

        //Replace 'viewmodels' in the moduleId with 'views' to locate the view.
        //Look for partial views in a 'views' folder in the root.
        viewLocator.useConvention();

        if (!security.zumoLocalCacheCheck()) {
            // No token - So Set the App Root to sign in or register
            system.log("Initiating App Root to Login...");
            app.setRoot('viewmodels/login', 'entrance');
        }
        else {
            security.zumoLocalCache().then(function () {
                system.log("Initiating App Root to Shell...");
                app.setRoot('viewmodels/shell', 'entrance');
            });

        }

    });

});