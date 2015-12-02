define(['jquery.dcjqaccordion', 'jquery.scrollTo', 'jquery.nicescroll', 'slidebars', 'bootstrap', 'bootstrap-datetimepicker', 'jquery.fileDownload'], function () {

    return {

        smoothScrollTop: function () { $("html, body").animate({ scrollTop: 0 }, "slow"); },

        initAudio: function(){
            window.alertHandle.src = '../lib/roadzen/audio/railalert.mp3';
            window.whistleHandle.src = '../lib/roadzen/audio/railwhistle.mp3';
        },

        // Right Slidebar Initiator
        initSlidebars: function () {
            $(function () {
                $.slidebars();
            });
        },
        initTooltips: function(){

            //    tool tips
            $('.tooltips').tooltip();
    
        },
        initPopovers: function(){

            //    popovers
            $('.popovers').popover();

        },
        // Nice Scrollbars Initiator
        initScrollbars: function () {            
            $("#sidebar").niceScroll({ styler: "fb", cursorcolor: "#e8403f", cursorwidth: '3', cursorborderradius: '10px', background: '#404040', spacebarenabled: false, cursorborder: '' });
            $("html").niceScroll({
                styler: "fb",
                autohidemode: 'false',
                cursorcolor: "#e8403f",
                cursorwidth: '12px',
                cursorborderradius: '10px',
                background: '#404040',
                spacebarenabled: false,
                cursorborder: '',
                zindex: '1000'
            });
        },
        resizeScrollbars: function (){ $("html").getNiceScroll().resize(); },
        // Left Sidebar Accorrdian Initiator
        initSidebarAccordian: function () {
            $(function () {
                $('#nav-accordion').dcAccordion({
                    eventType: 'click',
                    autoClose: true,
                    saveState: true,
                    disableLink: true,
                    speed: 'slow',
                    showCount: false,
                    autoExpand: true,
                    //        cookie: 'dcjq-accordion-1',
                    classExpand: 'dcjq-current-parent'
                });
            });
        },
        // Sidebar Responsive View Initiator
        initSidebarResponsive: function () {
            $(function () {
                function responsiveView() {
                    var wSize = $(window).width();
                    if (wSize <= 768) {
                        $('#container').addClass('sidebar-close');
                        $('#sidebar > ul').hide();
                    }

                    if (wSize > 768) {
                        $('#container').removeClass('sidebar-close');
                        $('#sidebar > ul').show();
                    }
                }
                $(window).on('load', responsiveView);
                $(window).on('resize', responsiveView);
            });
        },
        // Sidebar Toggle 
        toggleSidebar: function () {
            if ($('#sidebar > ul').is(":visible") === true) {
                $('#main-content').css({
                    'margin-left': '0px'
                });
                $('#sidebar').css({
                    'margin-left': '-210px'
                });
                $('#sidebar > ul').hide();
                $("#container").addClass("sidebar-closed");
            } else {
                $('#main-content').css({
                    'margin-left': '210px'
                });
                $('#sidebar > ul').show();
                $('#sidebar').css({
                    'margin-left': '0'
                });
                $("#container").removeClass("sidebar-closed");
            }
        },

        setDateTimePickers: function () {
            $(".form_datetime-component").datetimepicker({
                format: "MM d, yyyy H:ii p",
                autoclose: true,
                todayBtn: false,
                pickerPosition: "bottom-left"
            });
        }

    };

    var Script = function () {

        //    sidebar dropdown menu auto scrolling

        jQuery('#sidebar .sub-menu > a').click(function () {
            var o = ($(this).offset());
            diff = 250 - o.top;
            if (diff > 0)
                $("#sidebar").scrollTo("-=" + Math.abs(diff), 500);
            else
                $("#sidebar").scrollTo("+=" + Math.abs(diff), 500);
        });

        // widget tools

        jQuery('.panel .tools .fa-chevron-down').click(function () {
            var el = jQuery(this).parents(".panel").children(".panel-body");
            if (jQuery(this).hasClass("fa-chevron-down")) {
                jQuery(this).removeClass("fa-chevron-down").addClass("fa-chevron-up");
                el.slideUp(200);
            } else {
                jQuery(this).removeClass("fa-chevron-up").addClass("fa-chevron-down");
                el.slideDown(200);
            }
        });

        // by default collapse widget

        //    $('.panel .tools .fa').click(function () {
        //        var el = $(this).parents(".panel").children(".panel-body");
        //        if ($(this).hasClass("fa-chevron-down")) {
        //            $(this).removeClass("fa-chevron-down").addClass("fa-chevron-up");
        //            el.slideUp(200);
        //        } else {
        //            $(this).removeClass("fa-chevron-up").addClass("fa-chevron-down");
        //            el.slideDown(200); }
        //    });

        jQuery('.panel .tools .fa-times').click(function () {
            jQuery(this).parents(".panel").parent().remove();
        });

        
        


    }();

});

