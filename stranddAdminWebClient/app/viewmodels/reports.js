define(['durandal/system', 'plugins/http', 'plugins/router', 'durandal/app', 'roadzen/core-ui', 'app-settings', 'roadzen/core-security', 'knockout', 'moment'], function (system, http, router, app, ui, settings, security, ko, moment) {
  
    //ViewModel State Behavior
    //var stateLoading = ko.observable("Staff Assignments List Loading...");

    //Report Generator Paramaters
    var listTimeZoneSelection = ko.observableArray(listTimeZoneSelection);
    var selectedTimeZone = ko.observable();
    var selectedStartTime = ko.observable();
    var selectedEndTime = ko.observable();

    var cssGenerateButton = ko.observable('btn btn-default btn-lg');
    var markupGenerateButton = ko.observable('<i class="fa fa-file-excel-o"></i> Generate Report');
    var disableGenerateButton = ko.observable(false);

    var parameterStartTime = ko.computed({
        read: function () {
            var returnTimeZone = ko.utils.arrayFirst(listTimeZoneSelection(), function (zoneentry) { return zoneentry.Id === selectedTimeZone(); });
            if (selectedStartTime() && returnTimeZone) {
                var utcAddend = returnTimeZone.BaseUtcOffset
                utcAddend = utcAddend.substring(0, utcAddend.length - 3);
                if (utcAddend.substr(0, 1) != '-') { utcAddend = '+' + utcAddend; }
                system.log(["Creating TimeStamp for StartTime...", selectedStartTime(), utcAddend])
                return moment(selectedStartTime() + utcAddend).format();
            }
            else { return null; }
        }
    });

    var parameterEndTime = ko.computed({
        read: function () {            
            var returnTimeZone = ko.utils.arrayFirst(listTimeZoneSelection(), function (zoneentry) { return zoneentry.Id === selectedTimeZone(); });
            if (selectedEndTime() && returnTimeZone) {
                var utcAddend = returnTimeZone.BaseUtcOffset
                utcAddend = utcAddend.substring(0, utcAddend.length - 3);
                if (utcAddend.substr(0, 1) != '-') { utcAddend = '+' + utcAddend; }
                system.log(["Creating TimeStamp for EndTime...", selectedEndTime(), utcAddend])
                return moment(selectedEndTime() + utcAddend).format();
            }
            else { return null; }
        }
    });

    var urlIncidentHistory = ko.computed({
        read: function () {
            var urlString =  '?' + 'timezone=' + encodeURIComponent(selectedTimeZone());
            if (selectedStartTime()) { urlString += '&starttime=' + encodeURIComponent(parameterStartTime()); }
            if (selectedEndTime()) { urlString += '&endtime=' + encodeURIComponent(parameterEndTime()); }
            var requestFullURL = settings.getIncidentsExcelUrl() + urlString;
            system.log(["Creating URL for Excel Generatior...", requestFullURL]);
            return requestFullURL;
        },
    });

    //SCM Access Settings
    var url = settings.getSystemTimeZonesUrl();
    var qs = {};
    var headers = { "z-xumo-auth": security.getToken() };    

    var reports = {

        displayName: 'Reports',
        selectedTimeZone: selectedTimeZone,
        getServiceTimeZoneValues: getServiceTimeZoneValues,
        listTimeZoneSelection: listTimeZoneSelection,
        urlIncidentHistory: urlIncidentHistory,
        selectedStartTime: selectedStartTime,
        parameterStartTime: parameterStartTime,
        selectedEndTime: selectedEndTime,
        parameterEndTime: parameterEndTime,
        getExcelReport: getExcelReport,
        markupGenerateButton: markupGenerateButton,
        cssGenerateButton: cssGenerateButton,
        disableGenerateButton: disableGenerateButton,
        activate: activate,
        attached: attached
    };

    return reports;

    //Composition LifeCycle Methods
    function activate() { reports.getServiceTimeZoneValues(); }
    function attached() { ui.setDateTimePickers(); }

    //Bound Methods

    function getServiceTimeZoneValues() {

        http.get(url, qs, headers).then(function (response) {
            system.log(['System Time Zones', response]);
            reports.listTimeZoneSelection(response);
            reports.selectedTimeZone('India Standard Time');
        });

    }

    function getExcelReport() {
        system.log(["Passing Excel URL...", urlIncidentHistory()]);

        reports.markupGenerateButton('<i class="fa fa-cog fa-pulse"></i> Generating Report...');
        reports.cssGenerateButton('btn btn-primary btn-lg');
        reports.disableGenerateButton(true);
 
        $.fileDownload(urlIncidentHistory(), {
            successCallback: function (url) {
                system.log(["File Download Success...", url]);
                reports.markupGenerateButton('<i class="fa fa-check"></i> Generation Success');
                reports.cssGenerateButton('btn btn-success btn-lg');
                reports.disableGenerateButton(false);
            },
            failCallback: function (responseHtml, url) {
                system.log(["File Download Failure...", responseHtml, url]);
                reports.markupGenerateButton('<i class="fa fa-exclamation-circle"></i> Generation Failed');
                reports.cssGenerateButton('btn btn-danger btn-lg');
                reports.disableGenerateButton(false);
            }
        });
    }


});