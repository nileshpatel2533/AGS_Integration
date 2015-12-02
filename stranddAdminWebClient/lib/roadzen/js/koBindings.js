define(['durandal/system', 'ko', 'moment'], function (system, ko, moment) {

    return {

        registerBase: function () {

            //Binding for displaying Progress Bar
            ko.bindingHandlers.progressDisplay = {

                init: function (element) {

                    $("<div>").appendTo(element);

                },

                update: function (element, valueAccessor) {

                    var val = ko.unwrap(valueAccessor());

                    if (val.striped) { if (!$(element).hasClass("progress-striped")) { $(element).addClass("progress-striped"); } }
                    else { if ($(element).hasClass("progress-striped")) { $(element).removeClass("progress-striped"); } }

                    if (val.active) { if (!$(element).hasClass("active")) { $(element).addClass("active"); } }
                    else { if ($(element).hasClass("active")) { $(element).removeClass("active"); } }

                    $("div", element).removeClass();
                    $("div", element).addClass("progress-bar");
                    $("div", element).addClass(val.type);
                    $("div", element).css("width", val.progress);

                }
            };

            //Switch Binding
            ko.bindingHandlers.switchDisplay = {

                init: function (element, valueAccessor, allBindingsAccessor) {

                    var options = allBindingsAccessor().switchOptions || {};

                    $(element).append(

                        $("<div>").addClass("switch-animate")
                        .append("<input type='checkbox'>")
                        .append("<span class='switch-left'>" + "<i class=' " + options.textTrue + "'></i></span>")
                        .append("<label>&nbsp;</label>")
                        .append("<span class='switch-right'>" + "<i class=' " + options.textFalse + "'></i></span>")

                    );

                    $(element).click(function () { var value = valueAccessor(); value(!value()); });
                    
                },

                update: function (element, valueAccessor) {

                    
                    var val = ko.unwrap(valueAccessor());

                    if (val) { if (!$("div", element).hasClass("switch-on")) { $("div", element).addClass("switch-on"); } if ($("div", element).hasClass("switch-off")) { $("div", element).removeClass("switch-off"); }}
                    else { if ($("div", element).hasClass("switch-on")) { $("div", element).removeClass("switch-on"); } if (!$("div", element).hasClass("switch-off")) { $("div", element).addClass("switch-off"); }}

                }
            };

            //Binding for displaying the Calendar Time
            ko.bindingHandlers.calendarTime = {
                update: function (element, valueAccessor) {
                    var val = ko.unwrap(valueAccessor());
                    return ko.bindingHandlers.text.update(element, function () { return moment(val).calendar(); });
                }
            };

            //Binding for displaying the Expanded Time
            ko.bindingHandlers.expandedTime = {
                update: function (element, valueAccessor) {
                    var val = ko.unwrap(valueAccessor());
                    return ko.bindingHandlers.text.update(element, function () { return moment(val).format('MMMM Do YYYY, h:mm:ss a'); });
                }
            };

            //Binding for displaying the Passed Time Ago
            ko.bindingHandlers.agoTime = {
                update: function (element, valueAccessor) {
                    var val = ko.unwrap(valueAccessor());
                    return ko.bindingHandlers.text.update(element, function () { return moment(val).fromNow(); });
                }
            };
            


        },

    }

});