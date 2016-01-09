define(['app'], function (app) {
    app.controller('MainController',
	[
		'$rootScope',
		'$scope',
		'$http',
		'$route',
		function ($rootScope, $scope, $http, $route) {
		    console.log("Main controller is running");

		}
	]);

    app.directive('scrollToItem', function () {
        return {
            restrict: 'A',
            scope: {
                scrollTo: "@"
            },
            link: function (scope, $elm, attr) {

                $elm.on('click', function () {
                    $('html,body').animate({ scrollTop: $(scope.scrollTo).offset().top }, "slow");
                });
            }
        }
    });
     
});