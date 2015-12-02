define(['app-settings', 'breeze', 'knockout'], function (settings, breeze, ko) {

    var entityDefinition = new breeze.EntityType({
        shortName: "Account",
        namespace: "StrandD",
        defaultResourceName: "Accounts",
        dataProperties: {

            accountGUID: { dataType: "String", isPartOfKey: true },
            phone: { dataType: "String" },
            name: { dataType: "String" },
            email: { dataType: "String" },
            providerUserID: { dataType: "String" },
            registeredAt: { dataType: "DateTimeOffset" }

        },
        navigationProperties: {

            /*
            incidents: {
                entityTypeName: "Incident#StrandD", isScalar: false,
                associationName: "Incident_Account"
            }
            */
        }

    });



    var entityConstructor = function (incident) {

    }

    return {

        entityDefinition: entityDefinition,
        entityConstructor: entityConstructor

    };

});