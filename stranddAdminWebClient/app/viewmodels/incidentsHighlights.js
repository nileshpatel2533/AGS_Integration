define(['durandal/system', 'durandal/app', 'app-settings', 'viewmodels/incidents', 'knockout'], function (system, app, settings, incidents, ko) {
    
    //Settings for Highlights
    var indexStatusCode = settings.getStatusCodeIndex();

    //ViewModel State Behavior
    var stateLoading = ko.observable("Data Highlights Loading or None...");

    //Computed Data
    var countStatus = ko.computed(function () {

        var statusCount = [];
        indexStatusCode.forEach( function (arrayItem) { statusCount.push({code:arrayItem.code, text:arrayItem.text, count:0}); });
        
        ko.utils.arrayForEach(incidents.log(), function (incident) { statusCount.forEach(function (countEntry) { if (countEntry.code == incident.statusCode()) { countEntry.count++;} }); });
        return statusCount;

    });

    var incidentsHighlights = {

        displayName: 'Incidents Highlights',
        stateLoading: stateLoading,
        countStatus: countStatus,
        activate: activate,
        attached: attached,

    };

    return incidentsHighlights;

    //Composition LifeCycle Methods
    function activate() {

        
    }

    function attached() {


    }

    //Bound Methods



});