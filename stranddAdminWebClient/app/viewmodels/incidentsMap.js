define(['durandal/system', 'durandal/app', 'app-settings', 'viewmodels/incidents', 'knockout'], function (system, app, settings, incidents, ko) {
  
    //Settings for Map
    var optionsMap = settings.getMapOptions();
    var optionsMarker = settings.getMarkerOptions();
    var optionsPresetPositions = ko.observableArray(settings.mapPresetPositionIndex);

    var stateLoading = ko.observable("Map Data Loading or None...");
    stateLoading.subscribe(function (newValue) { app.trigger('IncidentsMap:LoadingStateChange', newValue); });
    stateLoading.extend({ notify: 'always' });

    var currentPositionObj = ko.observable({ center: optionsMap.center, zoom: optionsMap.zoom });

    var incidentMap = {

        displayName: 'Incident Map',
        activate: activate,
        attached: attached,
        stateLoading: stateLoading,
        currentPositionObj: currentPositionObj,
        optionsMap: optionsMap,
        optionsMarker: optionsMarker,
        optionsPresetPositions: optionsPresetPositions,
        selectPreset: selectPreset

    };

    return incidentMap;

    //Composition LifeCycle Methods
    function activate() {

        
    }

    function attached() {

        //gmapManager.initialize();  
        //loadMapMarkers();
        var state = stateLoading();
        stateLoading(state);
        system.log(incidentMap.optionsPresetPositions());

    }

    //Bound Methods
    function selectPreset(preset) { incidentMap.currentPositionObj({ center: preset.center, zoom: preset.zoom }); }


});