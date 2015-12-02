//Require.JS Async Plugin Needed for Map API Scripts Loading

define(['durandal/system', 'plugins/router', 'knockout'], function (system, router, ko) {
    
    
    //Singleton Public Definition & Return
    var gmapManager = {

        registerMapBindings: registerMapBindings

    }
    return gmapManager;

    function resetMapCenterDOM(map, center) {        

        google.maps.event.trigger(map, "resize");
        map.setCenter(center);

    }

    function navigateIncident(incidentGUID) {

        system.log(["Incident GUID Passing -navigateIncident", incidentGUID]);
        if (router.activeInstruction().config.title == "Incidents") {

            require(['viewmodels/incidents'], function (incidents) {

                incidents.hashIncidentGUID(incidentGUID);
                incidents.changeCurrentIncident(incidentGUID);
                router.navigate('#incidents/' + incidentGUID, false);

            });

        }
        else { router.navigate(('#incidents/' + incidentGUID)); }

    }

    function registerMapBindings () {
        //Binding for mapping GMap to GMapManager
        ko.bindingHandlers.googleMap = {

            init: function (element, valueAccessor, allBindingsAccessor, data, context) {

                //initialize datepicker with some optional options
                var options = allBindingsAccessor().mapOptions || {};

                // Display a map on the page
                data.map = new google.maps.Map(element, options);
                data.map.setTilt(45);

                // Set Markers Hash Object
                data.markers = {};
                data.zoom = options.zoom;
                data.center = options.center;               

                google.maps.event.addDomListener(window, "resize", resetMapCenterDOM(data.map, data.center));                

            },

            update: function (element, valueAccessor, allBindingsAccessor, data, context) {

                //Prepare the Update Inputs and Subscriptions
                var val = ko.unwrap(valueAccessor());

                var positionTag = ko.unwrap(allBindingsAccessor().positionTag());
                var currentTag = ko.unwrap(allBindingsAccessor().currentTag());
                var loadingTag = ko.unwrap(allBindingsAccessor().loadingTag());

                var markerOptions = allBindingsAccessor().markerOptions;


                //Update The Marker Array Collection based upon Log 
                var arrayLength = val.length;

                for (var i = 0; i < arrayLength; i++) {

                    // Check to see if the log incident has Coordinates and then Process for Map
                    if (val[i].coordinateX() && val[i].coordinateY()) {

                        // Check to see if the Marker already created
                        if (!data.markers.hasOwnProperty(val[i].incidentGUID())) {

                            data.markers[val[i].incidentGUID()] = new google.maps.Marker({

                                map: data.map,
                                position: new google.maps.LatLng(val[i].coordinateX(), val[i].coordinateY()),
                                icon: markerOptions.mapIconStandard,
                                incidentGUID: val[i].incidentGUID()

                            });

                            google.maps.event.addListener(data.markers[val[i].incidentGUID()], 'click', function () { return navigateIncident(this.incidentGUID); });

                        }

                        // Clear and Set Animations / Icons /  Zoom / Center regarding Current Tag
                        if (val[i].incidentGUID() == currentTag) {

                            data.markers[currentTag].setAnimation(google.maps.Animation.BOUNCE);
                            data.markers[currentTag].setIcon(markerOptions.mapIconPrimary);

                            data.center = { lat: val[i].coordinateX(), lng: val[i].coordinateY() };
                            system.log(["Google Map Zoom Setting", data.map.getZoom()]);
                            if (data.map.getZoom() < 15) { data.zoom = 15; } else { data.zoom = data.map.getZoom(); }

                        }
                        else {

                            data.markers[val[i].incidentGUID()].setAnimation(null);
                            data.markers[val[i].incidentGUID()].setIcon(markerOptions.mapIconStandard);

                        }

                    }

                } //Marker Collection Updation Completed

                //Set Center to Initial if no Current Incident
                if (!currentTag) { data.zoom = positionTag.zoom; data.center = positionTag.center; }

                //Set to Determined Map Center & Zoom
                system.log(["GMap Manager Binding - Setting for Pan & Zoom", data.center, data.zoom]);
                google.maps.event.trigger(data.map, "resize");

                data.map.panTo(data.center);
                data.map.setZoom(data.zoom);

                //Redraw Map
                google.maps.event.trigger(data.map, "resize");

            } //Binding Update Call Completed


        }; //Binding Handler Definition Completed        

    }    

});

