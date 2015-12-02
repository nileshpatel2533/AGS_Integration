define(['durandal/system', 'plugins/http', 'plugins/router', 'durandal/app', 'app-settings', 'roadzen/core-security', 'knockout', 'services/dataContext'],
    function (system, http, router, app, settings, security, ko, dataContext) {


        
        //ViewModel State Behavior
        var stateLoading = ko.observable("CostingSlab List Loading...");

        //SCM Access Settings
        //var slaburl = settings.getCostingSlabByIDUrl();
        var qs = {};      

        var headers = { "z-xumo-auth": security.getToken() };

        var logCore = ko.observableArray();
        var timestampUpdate = ko.observable();
        
     
        var costingslab = {

            displayName: 'Costing Slab Update',
            stateLoading: stateLoading,
            logCore: logCore,
            refreshLogCore: refreshLogCore,
            timestampUpdate: timestampUpdate,
            activate: activate,
            updateCostingSlab:updateCostingSlab,
            attached: attached

        };

        return costingslab;

        function activate() {
           
            costingslab.refreshLogCore();
        }

        function attached() {


        }

        //Bound Methods

        function refreshLogCore() {
          
            var slaburl = settings.getCostingSlabByIDUrl();
          
            http.get(slaburl, qs, headers).then(function (response) {
               
                response.forEach(function (entry) { if (!entry.hasOwnProperty('providerUserID')) { entry.providerUserID = null; } });
                system.log(['CostingSlab Update Log', response]);
                costingslab.logCore(response);
                costingslab.timestampUpdate(Date.now());
                stateLoading(false);
               
            });



        }

        function updateCostingSlab(request, data, event) {

            var detailsRequest = _.transform(request, function (result, val, key) { result[key] = val; });
            system.log(detailsRequest);
            dataContext.updateCostingSlab(detailsRequest, "COASTING-UPDATE");
            window.location.href = 'index.html#costingslab';
        }

    });