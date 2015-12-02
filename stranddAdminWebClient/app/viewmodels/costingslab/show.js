define(['durandal/system', 'plugins/http', 'plugins/router', 'durandal/app', 'app-settings', 'roadzen/core-security', 'knockout'],
    function (system, http, router, app, settings, security, ko) {




    //ViewModel State Behavior
    var stateLoading = ko.observable("CostingSlab List Loading...");

        //SCM Access Settings
  
    var url = settings.getCostingSlabListUrl();
    var qs = {};
    var headers = { "z-xumo-auth": security.getToken() };

    //Main Data
    var logCore = ko.observableArray();
    var timestampUpdate = ko.observable();
    var currentSlab = ko.observable();
   
    var costingslab = {

        displayName: 'Costing Slab',
        stateLoading: stateLoading,
        logCore: logCore,
        refreshLogCore: refreshLogCore,
        timestampUpdate: timestampUpdate,
        activate: activate,
        attached: attached,
      
           

    };

    return costingslab;

    

    //Composition LifeCycle Methods
    function activate() {
     
        costingslab.refreshLogCore();
    }

    function attached() {


    }

    //Bound Methods

    function refreshLogCore() {

        http.get(url, qs, headers).then(function (response) {
            // alert(response);
            response.forEach(function (entry) { if (!entry.hasOwnProperty('providerUserID')) { entry.providerUserID = null; } });
            system.log(['CostingSlab Log', response]);
            costingslab.logCore(response);
            costingslab.timestampUpdate(Date.now());
            stateLoading(false);

        });



    }

   
   


});