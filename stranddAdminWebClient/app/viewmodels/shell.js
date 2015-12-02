define(['durandal/system', 'plugins/router', 'durandal/app', 'roadzen/core-ui', 'app-settings', 'roadzen/core-security', 'services/liveDataConnection', 'services/incidentHubAdapter', 'services/dataContext', 'services/notificationHandler', 'knockout'], function (system, router, app, ui, settings, security, liveDataConnection, incidentHubAdapater, dataContext, notificationHandler, ko) {

    //App Version
    var appVersion = ko.observable(settings.getAppVersion());

    var stateSignalRConnect = ko.observable(false);

    var connectedOperatorList = ko.computed(function () {
        var list = incidentHubAdapater.connectedUserList();
        return list;
    });

    var loginName = ko.observable(security.getName());
    var loginEmail = ko.observable(security.getEmail());

    var viewSidebar = ko.observable("shellSidebar.html");
    viewSidebar.subscribe(function (newValue) { app.trigger('ShellUI:SidebarViewChange', newValue); });

    var styleMainContentLeftMargin = ko.observable("210px");
    styleMainContentLeftMargin.subscribe(function (newValue) { app.trigger('ShellUI:MainContentLeftMarginStyleChange', newValue); });

    var shell = {

        appVersion: appVersion,
        stateSignalRConnect: stateSignalRConnect,
        styleMainContentLeftMargin: styleMainContentLeftMargin,
        viewSidebar: viewSidebar,
        router: router,
        activate: activate,
        attached: attached,
        toggleSidebar: toggleSidebar,
        loginName: loginName,
        loginEmail: loginEmail,
        attemptLogout: attemptLogout,
        attemptRestart: attemptRestart,
        connectedOperatorList: connectedOperatorList

    };

    return shell;

    function activate() {

        //Initiliaze App Services
        initServices();

        //SetUp Router
        router.map([
            { route: '', title: 'Welcome', moduleId: 'viewmodels/welcome', nav: true, icon: 'fa-info' },
            { route: 'incidents', title: 'Incidents', moduleId: 'viewmodels/incidents', nav: true, icon: 'fa-road', requiredRoles: ['DIRECTOR', 'OPERATOR'] },
            { route: 'incidents/:id', title: 'Incidents', moduleId: 'viewmodels/incidents', nav: false, requiredRoles: ['DIRECTOR', 'OPERATOR'] },
            { route: 'payments', title: 'Payments', moduleId: 'viewmodels/payments', nav: true, icon: 'fa-credit-card', requiredRoles: ['DIRECTOR', 'OPERATOR', 'FINANCE'] },
            { route: 'reports', title: 'Reports', moduleId: 'viewmodels/excelreport', nav: true, icon: 'fa-file-text-o', requiredRoles: ['DIRECTOR'] },
            { route: 'communicationslog', title: 'Communications Log', moduleId: 'viewmodels/communicationsLog', nav: true, icon: 'fa-phone-square', requiredRoles: ['DIRECTOR', 'OPERATOR'] },
            { route: 'staff', title: 'Staff', moduleId: 'viewmodels/staff', nav: true, icon: 'fa-users', requiredRoles: ['DIRECTOR'] },
            { route: 'servicelog', title: 'Service Logs', moduleId: 'viewmodels/serviceLog', nav: true, icon: 'fa-server', requiredRoles: ['SYSADMIN'] },
            { route: 'costingslab', title: 'Costing Slab', moduleId: 'viewmodels/costingslab/show', nav: true, icon: 'fa-server', requiredRoles: ['SYSADMIN'] },
            { route: 'costingslabcreate', title: 'Costing Slab', moduleId: 'viewmodels/costingslab/create', nav: false, icon: 'fa-server', requiredRoles: ['SYSADMIN'] },
            { route: 'costingslabupdate/:id', title: 'Costing Slab', moduleId: 'viewmodels/costingslab/update', nav: false, icon: 'fa-server', requiredRoles: ['SYSADMIN'] }
        ]);

        router.guardRoute = function (routeInfo, params, instance) {
            if (typeof (params.config.requiredRoles) !== "undefined") {
                var res = false;
                for (var i = 0; i < params.config.requiredRoles.length; i++) {
                    if (security.checkRole(params.config.requiredRoles[i])) {
                        return true;
                        break;
                    }
                }
                if (!res) { system.log("Denied Role Access"); }
                return res;
            }
            else { return true; }
        };

        for (var i = 0; i < router.routes.length; i++) {
            if (typeof (router.routes[i].requiredRoles) !== "undefined" && router.routes[i].nav == true) {
                var res = false;
                for (var j = 0; j < router.routes[i].requiredRoles.length; j++) { if (security.checkRole(router.routes[i].requiredRoles[j])) { res = true; break; } }
                if (res) { router.routes[i].nav = true; } else { router.routes[i].nav = false; }
            }
        }

        router.buildNavigationModel();
        system.log('Router Mapped & Navigation Model Built');

        return router.activate();

    }
    function attached() {

        ui.initSlidebars();
        ui.initScrollbars();
        ui.initSidebarAccordian();
        ui.initAudio();
        //ui.initSidebarResponsive();
        //ui.initTooltips();
        //ui.initPopovers();

    }
    function toggleSidebar() {

        if (viewSidebar() == "") {
            viewSidebar("shellSidebar.html");
            shell.styleMainContentLeftMargin("210px");
        }
        else {
            viewSidebar("");
            shell.styleMainContentLeftMargin("0");
        }

        //ui.toggleSidebar();

    }

    function attemptRestart() {

        system.log("Restarting Application...");
        window.location.reload();

    }

    function initServices() {

        dataContext.initialize();
        notificationHandler.initialize();
        liveDataConnection.initialize();


    }

    function attemptLogout() {

        security.zumoLogout().done(function () {

            system.log("Successfully Logged-Out");
            router.deactivate();
            system.log("Setting Application Root to Restart...");
            app.setRoot('viewmodels/restart', 'entrance');

        });

    }

});