define(['app-settings', 'breeze', 'knockout'], function (settings, breeze, ko) {

    var entityDefinition = new breeze.EntityType({

        shortName: "Location",
        namespace: "StrandD",
        defaultResourceName: "Locations",
        dataProperties: {

            locationKey: { dataType: "String", isPartOfKey: true },
            rGDisplay : { dataType: "String" },
            x : { dataType: "Double" },
            y : { dataType: "Double" },
            landmark : { dataType: "String" },
            streetAddress : { dataType: "String" },
            city : { dataType: "String" },
            state : { dataType: "String" },
            zipCode: { dataType: "String" },
            country : { dataType: "String" }

        }

    });



    var entityConstructor = function (incident) {

    }

    return {

        entityDefinition: entityDefinition,
        entityConstructor: entityConstructor

    };

});