define(['app', 'directives/forms'], function (app) {
    app.controller('HomeController',
	[
		'$rootScope',
		'$scope',
		'$http',
		'$route',
        'API_URL',
        'limitToFilter',
		function ($rootScope, $scope, $http, $route, API_URL, limitToFilter) {

		    console.log("HomeController is running...");
		    $scope.Airports = null;

		    var _selected;

		    $scope.from = undefined;
		    $scope.to = undefined;

		    $http({
		        method: 'POST',
		        url: API_URL + 'GetAirports',
		    }).success(function (res) {
		        //$scope.countries = res.airports;
		        $scope.states = res.airports;
		    });

		    $scope.dynamicPopover = {
		        content: 'Hello, World!',
		        templateUrl: 'myPopoverTemplate.html',
		        title: 'Title'
		    };


            $scope.search = 
		    {
		        departure: "AYT",
		        arrival: "SAW",
		        flightType: "OW", //OW:Tek Yön | RT : Aktarmalı | MP : Gidiş Dönüş
		        paxCounts: [1,0,1,1,0,1],
		        departurDate: "16/03/2016",
                returnDate:"19/03/2016"
		    }


            $scope.getFlights = function() {
                $http({
                    method: 'GET',
                    url: API_URL + 'GetAir',
                    params:$scope.search
                }).success(function (res) {
                    console.log(res);
                });
            }



		    //JQuery Functions
		    //------------------------------
		    //Picker
		    //------------------------------

		    jQuery(function () {
		        "use strict";
		        jQuery("#datepickerStart").datepicker({
		            minDate:0
		        });
		        jQuery("#datepickerEnd").datepicker();
		    });


		    //------------------------------
		    //Custom Select
		    //------------------------------
		    jQuery(document).ready(function () {
		        "use strict";
		        jQuery('.mySelectBoxClass').customSelect();

		        /* -OR- set a custom class name for the stylable element */
		        //jQuery('.mySelectBoxClass').customSelect({customClass:'mySelectBoxClass'});
		    });

		    function mySelectUpdate() {
		        "use strict";
		        setTimeout(function () {
		            jQuery('.mySelectBoxClass').trigger('update');
		        }, 200);
		    }

		    jQuery(window).resize(function () {
		        mySelectUpdate();
		    });


		    //------------------------------
		    //CaroufredSell
		    //------------------------------
		    jQuery(document).ready(function (jQuery) {
		        "use strict";
		        jQuery("#foo").carouFredSel({
		            width: "100%",
		            height: 240,
		            items: {
		                visible: 5,
		                minimum: 1,
		                start: 2
		            },
		            scroll: {
		                items: 1,
		                easing: "easeInOutQuad",
		                duration: 500,
		                pauseOnHover: true
		            },
		            auto: false,
		            prev: {
		                button: "#prev_btn",
		                key: "left"
		            },
		            next: {
		                button: "#next_btn",
		                key: "right"
		            },
		            swipe: true
		        });


		        jQuery("#foo2").carouFredSel({
		            width: "100%",
		            height: 240,
		            items: {
		                visible: 5,
		                minimum: 1,
		                start: 2
		            },
		            scroll: {
		                items: 1,
		                easing: "easeInOutQuad",
		                duration: 500,
		                pauseOnHover: true
		            },
		            auto: false,
		            prev: {
		                button: "#prev_btn2",
		                key: "left"
		            },
		            next: {
		                button: "#next_btn2",
		                key: "right"
		            },
		            swipe: true
		        });


		    });




		    //------------------------------
		    //Nice Scroll
		    //------------------------------
		    jQuery(document).ready(function () {
		        "use strict";
		        var nice = jQuery("html").niceScroll({
		            cursorcolor: "#ccc",
		            cursorborder: "0px solid #fff",
		            railpadding: { top: 0, right: 0, left: 0, bottom: 0 },
		            cursorwidth: "5px",
		            cursorborderradius: "0px",
		            cursoropacitymin: 0,
		            cursoropacitymax: 0.7,
		            boxzoom: true,
		            autohidemode: false
		        });

		        jQuery("#air").niceScroll({ horizrailenabled: false });
		        jQuery("#hotel").niceScroll({ horizrailenabled: false });
		        jQuery("#car").niceScroll({ horizrailenabled: false });
		        jQuery("#vacations").niceScroll({ horizrailenabled: false });

		        jQuery("#air2").niceScroll({ horizrailenabled: false });
		        jQuery("#hotel2").niceScroll({ horizrailenabled: false });
		        jQuery("#car2").niceScroll({ horizrailenabled: false });
		        jQuery("#vacations2").niceScroll({ horizrailenabled: false });
		        jQuery("#flighthotel2").niceScroll({ horizrailenabled: false });
		        jQuery("#cruise2").niceScroll({ horizrailenabled: false });
		        jQuery("#hotelcar2").niceScroll({ horizrailenabled: false });
		        jQuery("#flighthotelcar2").niceScroll({ horizrailenabled: false });




		        jQuery('html').addClass('no-overflow-y');

		    });




		    //------------------------------
		    //Slider parallax effect
		    //------------------------------

		    jQuery(document).ready(function (jQuery) {
		        "use strict";
		        var jQueryscrollTop;
		        var jQueryheaderheight;
		        var jQueryloggedin = false;

		        if (jQueryloggedin == false) {
		            jQueryheaderheight = jQuery('.navbar-wrapper2').height() - 20;
		        } else {
		            jQueryheaderheight = jQuery('.navbar-wrapper2').height() + 100;
		        }


		        jQuery(window).scroll(function () {
		            "use strict";
		            var jQueryiw = jQuery('body').innerWidth();
		            jQueryscrollTop = jQuery(window).scrollTop();
		            if (jQueryiw < 992) {

		            }
		            else {
		                jQuery('.navbar-wrapper2').css({ 'min-height': 110 - (jQueryscrollTop / 2) + 'px' });
		            }
		            jQuery('#dajy').css({ 'top': ((-jQueryscrollTop / 5) + jQueryheaderheight) + 'px' });
		            //jQuery(".sboxpurple").css({'opacity' : 1-(jQueryscrollTop/300)});
		            jQuery(".scrolleffect").css({ 'top': ((-jQueryscrollTop / 5) + jQueryheaderheight) + 30 + 'px' });
		            jQuery(".tp-leftarrow").css({ 'left': 20 - (jQueryscrollTop / 2) + 'px' });
		            jQuery(".tp-rightarrow").css({ 'right': 20 - (jQueryscrollTop / 2) + 'px' });
		        });

		    });



		    //------------------------------
		    //SCROLL ANIMATIONS
		    //------------------------------	

		    jQuery(window).scroll(function () {
		        "use strict";
		        var jQueryiw = jQuery('body').innerWidth();

		        if (jQuery(window).scrollTop() != 0) {
		            jQuery('.mtnav').stop().animate({ top: '0px' }, 500);
		            jQuery('.logo').stop().animate({ width: '100px' }, 100);

		        }
		        else {
		            if (jQueryiw < 992) {
		            }
		            else {
		                jQuery('.mtnav').stop().animate({ top: '30px' }, 500);
		            }


		            jQuery('.logo').stop().animate({ width: '190px' }, 100);

		        }


		        //Social
		        if (jQuery(window).scrollTop() >= 300) {
		            jQuery('.social1').stop().animate({ top: '0px' }, 100);

		            setTimeout(function () {
		                jQuery('.social2').stop().animate({ top: '0px' }, 100);
		            }, 100);

		            setTimeout(function () {
		                jQuery('.social3').stop().animate({ top: '0px' }, 100);
		            }, 200);

		            setTimeout(function () {
		                jQuery('.social4').stop().animate({ top: '0px' }, 100);
		            }, 300);

		            setTimeout(function () {
		                jQuery('.gotop').stop().animate({ top: '0px' }, 200);
		            }, 400);

		        }
		        else {
		            setTimeout(function () {
		                jQuery('.gotop').stop().animate({ top: '100px' }, 200);
		            }, 400);
		            setTimeout(function () {
		                jQuery('.social4').stop().animate({ top: '-120px' }, 100);
		            }, 300);
		            setTimeout(function () {
		                jQuery('.social3').stop().animate({ top: '-120px' }, 100);
		            }, 200);
		            setTimeout(function () {
		                jQuery('.social2').stop().animate({ top: '-120px' }, 100);
		            }, 100);

		            jQuery('.social1').stop().animate({ top: '-120px' }, 100);

		        }


		    });


		    //------------------------------
		    //ROLLOVER
		    //------------------------------

		    var theSide = 'marginLeft';
		    var options = {};
		    options[theSide] = jQuery('.one').width() / 2 - 15;
		    jQuery(".one")
                .mouseenter(function () {
                    jQuery(".mhover", this).addClass("block");
                    jQuery(".mhover", this).removeClass("none");
                    jQuery(".icon", this).stop().animate(options, 100);
                })
		    jQuery(".one").mouseleave(function () {
		        jQuery(".mhover", this).addClass("none");
		        jQuery(".mhover", this).removeClass("block");
		        jQuery(".icon", this).stop().animate({ marginLeft: "0px" }, 100);
		    });


		}
	]);



});