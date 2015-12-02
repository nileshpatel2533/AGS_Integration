define(['durandal/system', 'plugins/router', 'durandal/app', 'durandal/events', 'roadzen/core-ui', 'knockout', 'toastr', 'gritter'], function (system, router, app, events, ui, ko, toastr) {

    var notificationHandler = {
        initialize: initialize,
        updateCallRequestNotificationButton: updateCallRequestNotificationButton
    };
    return notificationHandler;

    function updateCallRequestNotificationButton(communicationGUID, buttonState) {
        
        system.log(["Changing Button State...", communicationGUID, buttonState])
        var selector = '#notificationButton_' + communicationGUID;
        if (buttonState == "Operator") {
            $(selector).html('<i class="fa fa-phone-square"></i> You are Confirmed.');
            $(selector).prop('disabled', true);
            $(selector).removeClass('btn-default').addClass('btn-success');
        } else {
            if (buttonState == "Already ") {
                $(selector).html('<i class="fa fa-phone-square"></i> Already Confirmed.');
                $(selector).prop('disabled', true);
                $(selector).removeClass('btn-default').addClass('btn-warning');
            }
            else {
                $(selector).html('<i class="fa fa-phone-square"></i> Unknown.');
                $(selector).prop('disabled', true);
                $(selector).removeClass('btn-default').addClass('btn-danger');
            }
        }

    }

    function initialize() {
       
        app.on('ServiceIncidentHub:ReceivedNewInboundSms').then(function (data) {
        
            var notificationString = "";
          
            if (data.Tag) { notificationString += "Please call  [" + data.Tag + "] " + "<br/>" + "[" + data.Text + "]" ; }

            var notificationText = notificationString +
                '<br/><br/><button onclick=require("durandal/app").trigger("NotificationHandler:InboundMessageCallRequestConfirm","' + data.CommunicationGUID + '") type="button" class="btn btn-default btn-lg" id="notificationButton_' + data.CommunicationGUID + '"><i class="fa fa-phone-square"></i> Confirm Call</button>';

            $.gritter.add({
                title: 'Please Call Customer',
                image: 'img/misc/phone.png',
                text: notificationText,
                position: 'top-left',
                sticky: true,
            });

            window.alertHandle.play();

        });

        app.on('ServiceIncidentHub:ReceivedNewCustomerClientExceptionContact').then(function (data) {

            var notificationString = "";

            if (data.Tag) { notificationString += "CONTACT CUSTOMER on Phone [" + data.Tag + "] "; }
            if (data.IncidentGUID != "NONE") { notificationString += " | Exception on Incident [" + data.tag + "]"; }
           

            var notificationText = 'The customer is either unable to connect to network or has experienced a device malfunction and requests a phone call.<br/><br/><strong>'
                                    + notificationString + '</strong>' +
                                    '<br/><br/><button onclick=require("durandal/app").trigger("NotificationHandler:CurrentOperatorCallRequestConfirm","' + data.CommunicationGUID + '") type="button" class="btn btn-default btn-lg" id="notificationButton_' + data.CommunicationGUID + '"><i class="fa fa-phone-square"></i> Confirm Call</button>';
            /*
            var notificationText = 'The customer is either unable to connect to network or has experienced a device malfunction and requests a phone call.<br/><br/><strong>'
                                    + notificationString + '</strong>' +
                                    '<br/><br/><button onclick=submitCurrentOperatorCallRequestConfirm("' + data.CommunicationGUID + '") type="button" class="btn btn-default btn-lg"><i class="fa fa-phone-square"></i> Confirm Call</button>';
                                    */
/*
            if (linkString) { notificationText += "<br/><a href= #incidents/" + incidentGUID +">Existing Incident</a>"; }
*/

            $.gritter.add({
                title: 'Please Call Customer',
                image: 'img/misc/phone.png',
                text: notificationText,
                position: 'top-left',
                sticky: true,
            });

            window.alertHandle.play();

        });


       
        app.on('IncidentHubAdapter:ManualInvoiceTriggerSubmitted').then(function (data) {
            if (data.substring(0, 18) == "Incident not found") { $.gritter.add({ title: 'Incident not Found', text: 'The status update has NOT been processed by the Service', position: 'bottom-right', time: 3500 }); }
            else { $.gritter.add({ title: 'Request Successful', text: data, position: 'bottom-right', time: 2500 }); }
        });

        app.on('IncidentHubAdapter:StatusUpdateSubmitted').then(function (data) {
            if (data.substring(0, 18) == "Incident not found") { $.gritter.add({ title: 'Incident not Found', text: 'The status update has NOT been processed by the Service', position: 'bottom-right', time: 3500 }); }
            else { $.gritter.add({ title: 'Update Successful', text: 'The status update has been processed by the Service', position: 'bottom-right', time: 1500 }); }
        });        

        app.on('IncidentHubAdapter:DetailsUpdateSubmitted').then(function (data) {
            if (data.substring(0, 18) == "Incident not found") { $.gritter.add({ title: 'Incident not Found', text: 'The staff details update has NOT been processed by the Service', position: 'bottom-right', time: 3500 }); }
            else { $.gritter.add({ title: 'Update Successful', text: 'The staff details update has been processed by the Service', position: 'bottom-right', time: 1500 }); }
        });

        app.on('IncidentHubAdapter:CostingUpdateSubmitted').then(function (data) {
            if (data.substring(0, 18) == "Incident not found") { $.gritter.add({ title: 'Incident not Found', text: 'The costing update has NOT been processed by the Service', position: 'bottom-right', time: 3500 }); }
            else { $.gritter.add({ title: 'Update Successful', text: 'The costing details update has been processed by the Service', position: 'bottom-right', time: 1500 }); }
        });

        app.on('IncidentHubAdapter:OperatorUpdateSubmitted').then(function (data) {
            if (data.substring(0, 18) == "Incident not found") { $.gritter.add({ title: 'Incident not Found', text: 'The operator reassignment update has NOT been processed by the Service', position: 'bottom-right', time: 3500 }); }
            else { $.gritter.add({ title: 'Update Successful', text: 'The operator reassignment update has been processed by the Service', position: 'bottom-right', time: 1500 }); }
        });

        
        app.on('ServiceIncidentHub:ReceivedNewIncidentCustomer').then(function (data) { toastIncidentInfo("New Incident", "Submitted by Customer", 5000, data.IncidentGUID); window.whistleHandle.play(); });
        app.on('ServiceIncidentHub:ReceivedIncidentOperatorAdmin').then(function (data) { toastIncidentInfo("Incident Operator", "Modified by Admin", 3000, data.IncidentGUID); });
        app.on('ServiceIncidentHub:ReceivedIncidentDetailsAdmin').then(function (data) { toastIncidentInfo("Incident Details", "Modified by Admin", 2000, data.IncidentGUID); });
        app.on('ServiceIncidentHub:ReceivedIncidentStatusAdmin').then(function (data) { toastIncidentInfo("Incident Status", "Modified by Admin", 2000, data.IncidentGUID); });
        app.on('ServiceIncidentHub:ReceivedIncidentStatusCustomerCancel').then(function (data) { toastIncidentWarning("Incident Cancellation", "Submitted by Customer", 8000, data.IncidentGUID); });
        app.on('ServiceIncidentHub:ReceivedIncidentRatingCustomer').then(function (data) { toastIncidentInfo("Incident Rating", "Submitted by Customer", 5000, data.IncidentGUID); });
        app.on('ServiceIncidentHub:ReceivedIncidentCostingAdmin').then(function (data) { toastIncidentInfo("Incident Costing", "Modified by Admin", 2000, data.IncidentGUID); });
        app.on('ServiceIncidentHub:ReceivedNewPayment').then(function (data) {
            if (data.Status == 'FAILED') { toastIncidentError("Payment Failure", "Processed by Service", 5000, data.IncidentGUID); }
            else { toastIncidentInfo("New Payment Received", "Processed by Service", 5000, data.IncidentGUID); }
        });

    }

    function toastIncidentSuccess(title, msg, timeout, incidentGUID) {

        toastr.options.onclick = function (incidentGUID) { navigateIncident(incidentGUID) };
        var toastrObj = { title: title, msg: msg, overrides: { timeOut: timeout } };
        toastr.success(toastrObj.msg, toastrObj.title, toastrObj.overrides);

    }

    function toastIncidentInfo(title, msg, timeout, incidentGUID) {

        toastr.options.onclick = function () { system.log(["Incident GUID Passing", incidentGUID]); navigateIncident(incidentGUID) };
        var toastrObj = { title: title, msg: msg, overrides: { timeOut: timeout } };
        toastr.info(toastrObj.msg, toastrObj.title, toastrObj.overrides);

    }

    function toastIncidentWarning(title, msg, timeout, incidentGUID) {

        toastr.options.onclick = function () { system.log(["Incident GUID Passing", incidentGUID]); navigateIncident(incidentGUID) };
        var toastrObj = { title: title, msg: msg, overrides: { timeOut: timeout } };
        toastr.warning(toastrObj.msg, toastrObj.title, toastrObj.overrides);

    }

    function toastIncidentError(title, msg, timeout, incidentGUID) {

        toastr.options.onclick = function () { system.log(["Incident GUID Passing", incidentGUID]); navigateIncident(incidentGUID) };
        var toastrObj = { title: title, msg: msg, overrides: { timeOut: timeout } };
        toastr.error(toastrObj.msg, toastrObj.title, toastrObj.overrides);

    }

    function navigateIncident(incidentGUID) {

        system.log(["Incident GUID Passing -navigateIncident", incidentGUID]);
        if (router.activeInstruction().config.title == "Incidents") {

            require(['viewmodels/incidents'], function (incidents) {

                incidents.hashIncidentGUID(incidentGUID);
                incidents.changeCurrentIncident(incidentGUID);
                router.navigate('#incidents/' + incidentGUID, false);
                ui.smoothScrollTop();

            });

        }
        else { router.navigate(('#incidents/' + incidentGUID)); }

    }

});