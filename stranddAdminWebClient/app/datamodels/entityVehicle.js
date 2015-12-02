define(['app-settings', 'breeze', 'knockout'], function (settings, breeze, ko) {

    var entityDefinition = new breeze.EntityType({
        shortName: "Vehicle",
        namespace: "StrandD",
        defaultResourceName: "Vehicles",
        dataProperties: {

            vehicleGUID : { dataType: "String", isPartOfKey: true },
            make : { dataType: "String" },
            model : { dataType: "String" },
            year: { dataType: "Int32" },
            color : { dataType: "String" },
            registrationNumber: { dataType: "String" },
            description : {dataType: "String"}

        }
    });

    var entityConstructor = function (incident) {

    }

    return {

        entityDefinition: entityDefinition,
        entityConstructor: entityConstructor

    };

});