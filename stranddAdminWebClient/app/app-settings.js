define(function () {

    //Global Parameters
    appVersion = window.appVersion;
    debugFlag = window.debugFlag;
    buildIteration = window.buildIteration;
    serviceEnvironment = window.serviceEnvironment;  
    versionDisplay = window.versionDisplay;

    //Enviornmental Particulars
    liveDataService = function () {
        if (window.serviceEnvironment == "PROD") { return "http://strandd.azure-mobile.net"; }
        if (window.serviceEnvironment == "DEV") { return "http://strandd-dev.azure-mobile.net"; }
        if (window.serviceEnvironment == "AGS") { return "http://agsstranddservices.azure-mobile.net"; }
        if (window.serviceEnvironment == "CTE") { return "http://strandd-ctedev.azure-mobile.net"; }
        if (window.serviceEnvironment == "LOCAL") { return "http://localhost:59786"; }
    }

    liveAppKey = function () {
        if (window.serviceEnvironment == "PROD") { return "JDukfqFffMquwmYgboVwNbmdhTFnzf85"; }
        if (window.serviceEnvironment == "DEV") { return "VxWDWdzIuAMRISLHCidxJSsZVAKndv86"; }
        if (window.serviceEnvironment == "AGS") { return "uYagFevNaRjClRMxeYhuYGedbgBqPw96"; }
        if (window.serviceEnvironment == "CTE") { return "rXbJsaSxCWntemtxEGRaNbtsvQJlrH36"; }
        if (window.serviceEnvironment == "LOCAL") { return "vOtfNTpgbSqgtmdVvTCKTeXNLozOBx16"; }
    }

    liveLogsURL = function () {
        if (window.serviceEnvironment == "PROD") { return "http://strandd.scm.azure-mobile.net/api/logs/recent"; }
        if (window.serviceEnvironment == "DEV") { return "http://strandd-dev.scm.azure-mobile.net/api/logs/recent"; }
        if (window.serviceEnvironment == "AGS") { return "http://agsstranddservices.azure-mobile.net/api/logs/recent"; }
        if (window.serviceEnvironment == "CTE") { return "http://strandd-ctedev.azure-mobile.net/api/logs/recent"; }
        if (window.serviceEnvironment == "LOCAL") { return "http://agsstranddservices.azure-mobile.net/api/logs/recent"; }
    }

    liveBaseCompanyGUID = function () {
        if (window.serviceEnvironment == "PROD") { return "4b5f5a50-638a-4921-a60c-beb5e12fdca5"; }
        if (window.serviceEnvironment == "DEV") { return "4b5f5a50-638a-4921-a60c-beb5e12fdca5"; }
        if (window.serviceEnvironment == "AGS") { return "4b5f5a50-638a-4921-a60c-beb5e12fdca5"; }
        if (window.serviceEnvironment == "CTE") { return "4b5f5a50-638a-4921-a60c-beb5e12fdca5"; }
        if (window.serviceEnvironment == "LOCAL") { return "4b5f5a50-638a-4921-a60c-beb5e12fdca5"; }
    }

    // Routes
    routeSCMLogs = "/api/servicelogs";
    routePayments = "/api/payments";  
    routeCurrentAccountInfo = "/api/accounts/current";
    routeCommunicationsLog = "/api/communications"
   
    routeIncidentsExcel = "/api/incidents/exceloutput";
  
    routeSignalR = "/signalr";

    routeSystemTimeZones = "/api/systemtimezones/"
    routeStaffList = "/api/staffassignments/";  // + /{companyGUID}
    routeCurrentAccountRoles = "/api/staffassignments/currentuser/";  // + /{companyGUID}
    
    routeCallRequestOperatorConfirm = "/api/communications/currentoperator/confirmcallrequest";

    //AGS Developement ***********
    routePaymentsExcel = "/api/payments/exceloutput";
    routeCommunicationsLogExcel = "/api/communication/exceloutput";
    routeAccountsExcel = "/api/accounts/exceloutput";
    getCostingSlabByID = "/api/costingslab/findbycostingslabid";
    routeCostingSlabList = "/api/costingslab/";
    //**************************8


    // Default Inactive Incident History Timespan
    defaultHistoryHours = 24;

    // Map Settings
    mapInitialCenter = { lat: 28.6454415, lng: 77.0907573 };
    mapInitialZoom = 8;
    mapTypeID = "roadmap";
    mapStyles = [{ "featureType": "all", "elementType": "all", "stylers": [{ "visibility": "on" }] }, { "featureType": "all", "elementType": "geometry", "stylers": [{ "visibility": "on" }] }, { "featureType": "all", "elementType": "geometry.fill", "stylers": [{ "visibility": "on" }] }, { "featureType": "all", "elementType": "geometry.stroke", "stylers": [{ "visibility": "on" }] }, { "featureType": "all", "elementType": "labels", "stylers": [{ "visibility": "on" }] }, { "featureType": "administrative", "elementType": "all", "stylers": [{ "visibility": "on" }] }, { "featureType": "administrative", "elementType": "labels.text.fill", "stylers": [{ "color": "#444444" }] }, { "featureType": "landscape", "elementType": "all", "stylers": [{ "color": "#f2f2f2" }] }, { "featureType": "poi", "elementType": "all", "stylers": [{ "visibility": "off" }] }, { "featureType": "road", "elementType": "all", "stylers": [{ "saturation": -100 }, { "lightness": 45 }] }, { "featureType": "road.highway", "elementType": "all", "stylers": [{ "visibility": "simplified" }] }, { "featureType": "road.highway.controlled_access", "elementType": "all", "stylers": [{ "visibility": "on" }] }, { "featureType": "road.arterial", "elementType": "labels.icon", "stylers": [{ "visibility": "off" }] }, { "featureType": "road.local", "elementType": "all", "stylers": [{ "visibility": "on" }] }, { "featureType": "transit", "elementType": "all", "stylers": [{ "visibility": "off" }] }, { "featureType": "transit.line", "elementType": "all", "stylers": [{ "visibility": "on" }] }, { "featureType": "transit.station", "elementType": "all", "stylers": [{ "visibility": "on" }] }, { "featureType": "transit.station.airport", "elementType": "all", "stylers": [{ "visibility": "on" }] }, { "featureType": "transit.station.bus", "elementType": "all", "stylers": [{ "visibility": "on" }] }, { "featureType": "transit.station.rail", "elementType": "all", "stylers": [{ "visibility": "on" }] }, { "featureType": "water", "elementType": "all", "stylers": [{ "color": "#ef8d66" }, { "visibility": "on" }] }];

    // Marker Settings
    mapIconStandard = "img/map/mapicon_info.png";
    mapIconPrimary = "img/map/mapicon_primary.png";

    //Details View Related Map Settings
    coordsLinkInitialZoom = 14;

    // Map Presets
    mapPresetPositionIndex = [
        { code: "GLOBE", text: "World", center: { lat: 0, lng: 0 }, zoom: 1, icon: "fa-globe" },
        { code: "INDIA", text: "India", center: { lat: 21.1289956, lng: 82.7792201 }, zoom: 5, icon: "fa-location-arrow" },
        { code: "DLH-NCR", text: "Delhi Metro NCR", center: { lat: 28.6469655, lng: 77.0932634 }, zoom: 10, icon: "fa-location-arrow" },
        { code: "PGH", text: "PGH Metro", center: { lat: 40.4313684, lng: -79.9805005 }, zoom: 10, icon: "fa-location-arrow" },
        { code: "NYC", text: "NYC Metro", center: { lat: 40.7033127, lng: -73.979681 }, zoom: 10, icon: "fa-location-arrow" }
    ]

    // Status Configuration
    statusCodeIndex = [        
        { sequence: -1000, code: "FAILED", text: "Service Failed", progress: "100%", defaultType: "progress-bar-danger", defaultAnimation: false },
        { sequence: -100, code: "DECLINED", text: "Request Declined", progress: "100%", defaultType: "progress-bar-warning", defaultAnimation: false },
        { sequence: -1, code: "CANCELLED", text: "Request Cancelled", progress: "100%", defaultType: "progress-bar-warning", defaultAnimation: false },
        { sequence: 1, code: "SUBMITTED", text: "Request Submitted", progress: "16%", defaultType: "progress-bar-info", defaultAnimation: false },
        { sequence: 2, code: "CONFIRMED", text: "Request Confirmed", progress: "32%", defaultType: "progress-bar-info", defaultAnimation: false },
        { sequence: 3, code: "PROVIDER-FOUND", text: "Provider Found", progress: "49%", defaultType: "progress-bar-success", defaultAnimation: false },
        //{ sequence: 4, code: "DISPATCHED", text: "Provider Dispatched", progress: "66%", defaultType: "progress-bar-success", defaultAnimation: true },
        { sequence: 4, code: "IN-PROGRESS", text: "Provider En Route", progress: "83%", defaultType: "progress-bar-success", defaultAnimation: true },
        { sequence: 100, code: "ARRIVED", text: "Provider Arrived", progress: "90%", defaultType: "progress-bar-success", defaultAnimation: false },
        { sequence: 1000, code: "COMPLETED", text: "Job Completed", progress: "100%", defaultType: "progress-bar-success", defaultAnimation: false }
    ];

    //Payment Status Configuration
    paymentStatusCodeIndex = [
        { code: "PAYMENT-FAIL", text: "Payment Failure" },
        { code: "PAYMENT-SUCCESS", text: "Payment Successful" },
        { code: "PAYMENT-CASH", text: "Cash Payment" },
    ];
    //Sunil Start
    serviceTypeCodeIndex = [

       { code: "ROADSIDE-ASSISTANCE-DAY", text: "Roadside Assistance Day", sequence: 1 },
       { code: "ROADSIDE-ASSISTANCE-NIGHT", text: "Roadside Assistance Night", sequence: 2 },
       { code: "CONVENTIONAL-TOWING", text: "Conventional Towing", sequence: 3 },
       { code: "FLATBED-TOWING", text: "Flatbed Towing", sequence: 4 }
    ];
    //Job Configuration
    jobImageBasePath = "img/job/";
    jobCodeIndex = [
        { code: "ACCIDENT", text: "Accident" },
        { code: "TOWING", text: "Need Towing" },
        { code: "BATTERY", text: "Dead Battery" },
        { code: "PUNCTURE-REPAIR", text: "Need puncture repair" },
        { code: "PUNCTURE-SPARE", text: "Change Spare wheel" },
        { code: "LOCKOUT-LOSTKEY", text: "Broken Key/ Lost Key" },
        { code: "LOCKOUT-KEYINCAR", text: "Locked Keys in Car" },
        { code: "FUEL-PETROL", text: "Empty Tank - Need Petrol" },
        { code: "FUEL-DIESEL", text: "Empty Tank - Need Diesel" },
        { code: "FUEL-WRONG", text: "Put Wrong Fuel In Vehicle" },
        { code: "OTHER-NOTSTART", text: "Car is not starting" },
        { code: "OTHER-BRAKE", text: "Brake Problem" },
        { code: "OTHER-CLUTCH", text: "Clutch Problem" },
        { code: "OTHER-STEERING", text: "Steering / Suspension Problem" },
        { code: "OTHER-LIGHTS", text: "Warning Lights on Cluster" }
    ];

    settings = {

        //Derived Settings
        getMapOptions: function () { return { mapTypeId: settings.mapTypeID, styles: settings.mapStyles, center: settings.mapInitialCenter, zoom: settings.mapInitialZoom }; },
        getMarkerOptions: function () { return { mapIconStandard: settings.mapIconStandard, mapIconPrimary: settings.mapIconPrimary }; },
        getStatusCodeIndex: function () { return settings.statusCodeIndex; },
        getServiceTypeCodeIndex: function () { return settings.serviceTypeCodeIndex; },

        //Derived Parameters
        getAppVersion: function () { return settings.appVersion; },
        getBuildIteration: function () { return settings.buildIteration; },
        getDebugFlag: function () { return settings.debugFlag; },
        getServiceEnvironment: function () { return settings.serviceEnvironment; },
        getVersionDisplay: function () { return settings.versionDisplay; },

        //Derived URLs
        getSystemTimeZonesUrl: function () { return settings.liveDataService() + settings.routeSystemTimeZones; },
        getOperatorCallRequestConfirmationUrl: function () { return settings.liveDataService() + settings.routeCallRequestOperatorConfirm; },
        getCommunicationsLogUrl: function () { return settings.liveDataService() + settings.routeCommunicationsLog; },
        getCurrentAccountInfoUrl: function () { return settings.liveDataService() + settings.routeCurrentAccountInfo; },
        getCurrentAccountRolesUrl: function () { return settings.liveDataService() + settings.routeCurrentAccountRoles + settings.liveBaseCompanyGUID(); },
        getPaymentsUrl: function () { return settings.liveDataService() + settings.routePayments; },
        getStaffListUrl: function () { return settings.liveDataService() + settings.routeStaffList + settings.liveBaseCompanyGUID(); },
        getSCMLogsUrl: function () { return settings.liveDataService() + settings.routeSCMLogs; },
        getSignalRConnectionUrl: function () { return settings.liveDataService() + settings.routeSignalR; },
        getIncidentsExcelUrl: function () {
            var baseURL = settings.liveDataService() + settings.routeIncidentsExcel;
            var outputURL = baseURL.substr(0, 7) + ('fileexcel:' + settings.liveAppKey() + '@') + baseURL.substr(7);
            return outputURL;
        },        

        getCostingSlabListUrl: function () { return settings.liveDataService() + settings.routeCostingSlabList; },

        getCostingSlabByIDUrl: function () {
            var qst = window.location.hash;
            var qid = "?CostingSlabid=" + qst.substring(qst.indexOf("/") + 1);
            return settings.liveDataService() + settings.getCostingSlabByID + qid;
        },

        // AGS Changes ********************
        getPaymentsExcelUrl: function () {
            var baseURL = settings.liveDataService() + settings.routePaymentsExcel;
            var outputURL = baseURL.substr(0, 7) + ('fileexcel:' + settings.liveAppKey() + '@') + baseURL.substr(7);
            return outputURL;
        },
        getCommunicationsLogExcelUrl: function () {
            var baseURL = settings.liveDataService() + settings.routeCommunicationsLogExcel;
            var outputURL = baseURL.substr(0, 7) + ('fileexcel:' + settings.liveAppKey() + '@') + baseURL.substr(7);
            return outputURL;
        },

        getAccountsExcelUrl: function () {
            var baseURL = settings.liveDataService() + settings.routeAccountsExcel;
            var outputURL = baseURL.substr(0, 7) + ('fileexcel:' + settings.liveAppKey() + '@') + baseURL.substr(7);
            return outputURL;
        },

        //**************************8



        //Global Parameters
        appVersion: appVersion,
        buildIteration: buildIteration,
        debugFlag: debugFlag,
        serviceEnvironment: serviceEnvironment,
        versionDisplay: versionDisplay,       

        //Enviornmental Particulars
        liveDataService: liveDataService,
        liveLogsURL: liveLogsURL,
        liveAppKey: liveAppKey,
        liveBaseCompanyGUID: liveBaseCompanyGUID,

        // Routes
        routeCurrentAccountInfo: routeCurrentAccountInfo,
        routeCurrentAccountRoles: routeCurrentAccountRoles,
        routePayments: routePayments,
        routeStaffList: routeStaffList,
        routeSCMLogs: routeSCMLogs,
        routeIncidentsExcel: routeIncidentsExcel,
        routeSignalR: routeSignalR,
        routeCommunicationsLog: routeCommunicationsLog,
        routeCallRequestOperatorConfirm: routeCallRequestOperatorConfirm,
        routeSystemTimeZones: routeSystemTimeZones,

        // AGS Changes **************************
        getCostingSlabByID: getCostingSlabByID,
        routeCostingSlabList: routeCostingSlabList,
        routePaymentsExcel: routePaymentsExcel,
        routeCommunicationsLogExcel: routeCommunicationsLogExcel,
        routeAccountsExcel: routeAccountsExcel,
        //****************************************8

        //Additional Settingss
        defaultHistoryHours: defaultHistoryHours,
        jobImageBasePath: jobImageBasePath,

        //Indices
        statusCodeIndex: statusCodeIndex,
        jobCodeIndex: jobCodeIndex,
        serviceTypeCodeIndex: serviceTypeCodeIndex,

        // Map Settings
        mapInitialCenter: mapInitialCenter,
        mapInitialZoom: mapInitialZoom,
        mapType: mapTypeID,
        mapStyles: mapStyles,
        mapIconStandard: mapIconStandard,
        mapIconPrimary: mapIconPrimary,
        coordsLinkInitialZoom: coordsLinkInitialZoom

    }

    return settings;

});