define(['durandal/system', 'app-settings', 'breeze', 'knockout'], function (system, settings, breeze, ko) {

    var entityDefinition = new breeze.EntityType({
        shortName: "Incident",
        namespace: "StrandD",
        defaultResourceName: "Incidents",
        dataProperties: {

            incidentGUID: { dataType: "String", isPartOfKey: true },
            jobCode: { dataType: "String" },
            customerComments: { dataType: "String" },
            staffNotes: { dataType: "String" },
            coordinateX: { dataType: "Double" },
            coordinateY: { dataType: "Double" },
            statusCode: { dataType: "String" },
            serviceFee: { dataType: "Decimal" },
            paymentMethod: { dataType: "String" },
            paymentAmount: { dataType: "Decimal" },
            rating: { dataType: "Int32" },
            concertoCaseID: { dataType: "String" },
            providerArrivalTime: { dataType: "DateTimeOffset" },
            createdAt: { dataType: "DateTimeOffset" },
            updatedAt: { dataType: "DateTimeOffset" },
            accountGUID: { dataType: "String" },
            vehicleGUID: { dataType: "String" },
            adminAccountGUID: { dataType: "String" },
            locationKey: { dataType: "String" },
            additionalDetails: { dataType: "String" },
            serviceType: { dataType: "String" },
            //kilometers: { dataType: "String" },
            calculatedBaseServiceCost: { dataType: "Decimal" },
            taxZoneRate: { dataType: "Decimal" },
            serviceKilometers: { dataType: "Decimal" },
            parkingCosts: { dataType: "Decimal" },
            tollCosts: { dataType: "Decimal" },
            otherCosts: { dataType: "Decimal" },
            offsetDiscount: { dataType: "Decimal" },
            calculatedSubtotal: { dataType: "Decimal" },
            calculatedTaxes: { dataType: "Decimal" },
            calculatedTotalCost: { dataType: "Decimal" }

        },
        navigationProperties: {


            account: {
                entityTypeName: "Account", isScalar: true,
                associationName: "Incident_Account", foreignKeyNames: ["accountGUID"]
            },
            admin: {
                entityTypeName: "Account", isScalar: true,
                associationName: "Incident_Admin", foreignKeyNames: ["adminAccountGUID"]
            },
            vehicle: {
                entityTypeName: "Vehicle", isScalar: true,
                associationName: "Incident_Vehicle", foreignKeyNames: ["vehicleGUID"]
            },
            location: {
                entityTypeName: "Location", isScalar: true,
                associationName: "Incident_Location", foreignKeyNames: ["locationKey"]
            }


        }
    });



    var entityConstructor = function (incident) {

        incident.eta = ko.observable();

        /*
        incident.marker = ko.computed(function () {
            if (incident.coordinateX() && incident.coordinateY()) {
                new google.maps.Marker({
                    position: new google.maps.LatLng(incident.coordinateX(), incident.coordinateY()),
                    map: window.map,
                    icon: settings.mapIconStandard
                });
            }
            else {
                return null;
            }
        });
        */

    }

    var augmentDisplays = function (incident) {

        incident.statusDisplayObj = ko.computed(function () {

            var returnStatusCode = incident.statusCode();

            //Pull Status Settings from DataModel based upon Code
            var returnStatusArray = settings.statusCodeIndex.filter(function (obj) {
                return obj.code == returnStatusCode;
            });

            //Return UNKNOWN Settings if Status Code not Found
            var returnStatusObj = returnStatusArray[0] || { text: "Unknown Status", code: "UNKNOWN", progress: "0%", defaultType: "progress-bar-info", defaultAnimation: false };

            //Default Progress Type
            returnStatusObj.type = returnStatusObj.defaultType;

            //Default Animation Set
            if (returnStatusObj.defaultAnimation) {
                returnStatusObj.striped = true;
                returnStatusObj.active = true;
            }
            else {
                returnStatusObj.striped = false;
                returnStatusObj.active = false;
            }

            //Conditional Logic for Additional Modification Parameters 
            //(TBD)

            return returnStatusObj;

        });

        incident.accountDisplay = ko.computed(function () {

            var returnDisplay = "";

            if (incident.accountGUID()) {
                if (incident.account_name()) { returnDisplay += incident.account_name(); }
                if (incident.account_phone()) { returnDisplay += " [" + incident.account_phone() + "] " }
                if (returnDisplay == "" && incident.account_providerUserID()) { returnDisplay = incident.account_providerUserID(); }
            }
            else { returnDisplay = "No Customer Associated"; }

            return returnDisplay;

        });

        incident.adminAccountDisplay = ko.computed(function () {

            var returnDisplay = "";

            if (incident.adminAccountGUID()) {
                if (incident.admin_name()) { returnDisplay = incident.admin_name(); }
                if (incident.admin_phone()) { returnDisplay += " [" + incident.admin_phone() + "] " }
                if (returnDisplay == "" && incident.admin_providerUserID()) { returnDisplay = incident.admin_providerUserID(); }
            }
            else { returnDisplay = "No Confirmed Operator"; }

            return returnDisplay;

        });

        incident.vehicleDisplay = ko.computed(function () {

            if (incident.vehicle_registrationNumber()) {

                var vehicleDetails = incident.vehicle_color() + " " + incident.vehicle_year() + " " + incident.vehicle_make() + " " + incident.vehicle_model();
                var vehicleRegistration = " [Registration #" + incident.vehicle_registrationNumber() + "]";
                return (vehicleDetails + vehicleRegistration);

            }
            else {

                return "No Vehicle Information";

            }

        });

        incident.jobDisplayObj = ko.computed(function () {
            var returnJobCode = incident.jobCode();

            //Pull Job Text from DataModel based upon Code
            var returnJobArray = settings.jobCodeIndex.filter(function (obj) {
                return obj.code == returnJobCode;
            });

            var returnJobObj = returnJobArray[0] || { text: "Unknown Job", code: "UNKNOWN" };
            returnJobObj.imagesrc = settings.jobImageBasePath + returnJobObj.code + '.png';

            return returnJobObj;
        });

        incident.locationDisplay = ko.computed(function () {

            var displayOutput = "";
            var addressDetails = "";
            var rgFormatted = "";

            /*
            if (incident.location_x() && incident.location_y) { displayOutput = "Coordinates: [" + incident.location_x() + "," + incident.location_y() + "]"; }
            else { displayOutput = "NO COORDINATES"; }
            */

            //entry.message = incident.location_rGDisplay().replace("NL Chars", '<br/>');
            rgFormatted += incident.location_rGDisplay();
            rgFormatted.replace(/\r?\n/g, "<br />");
            rgFormatted = rgFormatted.replace("Longitude", "| Longitude");
            displayOutput += rgFormatted;

            if (incident.location_landmark()) { addressDetails += "<br/>" + "Landmark: " + incident.location_landmark(); }
            if (incident.location_streetAddress()) { addressDetails += "<br/>" + "Street: " + incident.location_streetAddress(); }
            if (incident.location_city()) { addressDetails += "<br/>" + "City: " + incident.location_city(); }
            if (incident.location_state()) { addressDetails += "<br/>" + "State: " + incident.location_state(); }
            if (incident.location_zipCode()) { addressDetails += "<br/>" + "Zip: " + incident.location_zipCode(); }
            if (incident.location_country()) { addressDetails += "<br/>" + "Country: " + incident.location_zipCode(); }

            displayOutput += addressDetails;

            return displayOutput;

        });

        incident.coordsDisplayObj = ko.computed(function () {

            var coordsOutput = {};
            coordsOutput.text = "";
            coordsOutput.text += "[Lat: " +
                incident.coordinateX() +
                " , Lng: " +
                incident.coordinateY() +
                "]";

            //Initial Zoom Level
            coordsOutput.link = "";
            coordsOutput.link += "https://" +
                "www.google.com/maps/dir//" +
                incident.coordinateX() + "," +
                incident.coordinateY() + "/@" +
                incident.coordinateX() + "," +
                incident.coordinateY() + "," +
                settings.coordsLinkInitialZoom +
                "z?hl=en";

            return coordsOutput;

        });

        incident.concertoDisplay = ko.computed(function () {

            if (incident.concertoCaseID() && incident.concertoCaseID() != "0") { return ("Concerto Case #" + incident.concertoCaseID()); }
            else { return "No Concerto Case"; }

        });

        incident.costingDisplay = ko.computed(function () {

            if (incident.serviceType()) {
                var displayOutput = incident.serviceType();

                if (incident.serviceKilometers() != 0) { displayOutput += ' | Kilometers: ' + incident.serviceKilometers(); }
                if (incident.calculatedBaseServiceCost() != 0) { displayOutput += ' | Base Service Cost: ' + incident.calculatedBaseServiceCost(); }
                if (incident.parkingCosts() != 0) { displayOutput += ' | Parking Costs: ' + incident.parkingCosts(); }
                if (incident.tollCosts() != 0) { displayOutput += ' | Toll Costs: ' + incident.tollCosts(); }
                if (incident.otherCosts() != 0) { displayOutput += ' | Other Costs: ' + incident.otherCosts(); }
                if (incident.offsetDiscount() != 0) { displayOutput += ' | Discount: ' + incident.offsetDiscount(); }
                if (incident.calculatedSubtotal() != 0) { displayOutput += ' | Subtotal: ' + incident.calculatedSubtotal(); }
                if (incident.calculatedTaxes() != 0) { displayOutput += ' | Taxes: ' + incident.calculatedTaxes(); }

                if (incident.calculatedTotalCost() != 0) { displayOutput += ' | Total: ' + incident.calculatedTotalCost(); }

                return displayOutput;
            }
            else { return "No Costing Information"; }

            //"calculatedBaseServiceCost"
            //"taxZoneRate"            
        });

        return incident;

    }


    return {

        entityDefinition: entityDefinition,
        entityConstructor: entityConstructor,
        augmentDisplays: augmentDisplays


    };
});