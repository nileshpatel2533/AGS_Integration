/*!
 * ASP.NET SignalR JavaScript Library v2.1.1
 * http://signalr.net/
 *
 * Copyright Microsoft Open Technologies, Inc. All rights reserved.
 * Licensed under the Apache 2.0
 * https://github.com/SignalR/SignalR/blob/master/LICENSE.md
 *
 */

/// <reference path="..\..\SignalR.Client.JS\Scripts\jquery-1.6.4.js" />
/// <reference path="jquery.signalR.js" />
(function ($, window, undefined) {
    /// <param name="$" type="jQuery" />
    "use strict";

    if (typeof ($.signalR) !== "function") {
        throw new Error("SignalR: SignalR is not loaded. Please ensure jquery.signalR-x.js is referenced before ~/signalr/js.");
    }

    var signalR = $.signalR;

    function makeProxyCallback(hub, callback) {
        return function () {
            // Call the client hub method
            callback.apply(hub, $.makeArray(arguments));
        };
    }

    function registerHubProxies(instance, shouldSubscribe) {
        var key, hub, memberKey, memberValue, subscriptionMethod;

        for (key in instance) {
            if (instance.hasOwnProperty(key)) {
                hub = instance[key];

                if (!(hub.hubName)) {
                    // Not a client hub
                    continue;
                }

                if (shouldSubscribe) {
                    // We want to subscribe to the hub events
                    subscriptionMethod = hub.on;
                } else {
                    // We want to unsubscribe from the hub events
                    subscriptionMethod = hub.off;
                }

                // Loop through all members on the hub and find client hub functions to subscribe/unsubscribe
                for (memberKey in hub.client) {
                    if (hub.client.hasOwnProperty(memberKey)) {
                        memberValue = hub.client[memberKey];

                        if (!$.isFunction(memberValue)) {
                            // Not a client hub function
                            continue;
                        }

                        subscriptionMethod.call(hub, memberKey, makeProxyCallback(hub, memberValue));
                    }
                }
            }
        }
    }

    $.hubConnection.prototype.createHubProxies = function () {
        var proxies = {};
        this.starting(function () {
            // Register the hub proxies as subscribed
            // (instance, shouldSubscribe)
            registerHubProxies(proxies, true);

            this._registerSubscribedHubs();
        }).disconnected(function () {
            // Unsubscribe all hub proxies when we "disconnect".  This is to ensure that we do not re-add functional call backs.
            // (instance, shouldSubscribe)
            registerHubProxies(proxies, false);
        });

        proxies['incidentHub'] = this.createHubProxy('incidentHub');
        proxies['incidentHub'].client = {};
        proxies['incidentHub'].server = {
            assignIncidentOperator: function (assignmentRequest) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["AssignIncidentOperator"], $.makeArray(arguments)));
            },

            confirmMobileCustomerClientIncidentStatusUpdate: function (incidentGUID) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["ConfirmMobileCustomerClientIncidentStatusUpdate"], $.makeArray(arguments)));
            },

            getActiveIncidents: function () {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["GetActiveIncidents"], $.makeArray(arguments)));
            },

            getConnectedUserList: function () {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["GetConnectedUserList"], $.makeArray(arguments)));
            },

            getInactiveIncidents: function (historyHours) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["GetInactiveIncidents"], $.makeArray(arguments)));
            },

            getMobileCustomerClientIncidentStatusRequest: function (incidentGUID) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["GetMobileCustomerClientIncidentStatusRequest"], $.makeArray(arguments)));
            },

            mobileProviderClientJobAcceptanceTrigger: function (acceptanceStatus) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["MobileProviderClientJobAcceptanceTrigger"], $.makeArray(arguments)));
            },

            providerAcceptJob: function (jobTag) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["ProviderAcceptJob"], $.makeArray(arguments)));
            },

            sendIncidentCurrentInvoice: function (incidentGUID) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["SendIncidentCurrentInvoice"], $.makeArray(arguments)));
            },

            updateCosting: function (costingRequest) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["UpdateCosting"], $.makeArray(arguments)));
            },

            updateDetails: function (detailsRequest) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["UpdateDetails"], $.makeArray(arguments)));
            },

            updatePayment: function (paymentRequest) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["UpdatePayment"], $.makeArray(arguments)));
            },

            updateStatus: function (statusRequest) {
                return proxies['incidentHub'].invoke.apply(proxies['incidentHub'], $.merge(["UpdateStatus"], $.makeArray(arguments)));
            }
        };

        return proxies;
    };

    signalR.hub = $.hubConnection("/signalr", { useDefaultPath: false });
    $.extend(signalR, signalR.hub.createHubProxies());

}(window.jQuery, window));