define(['routes', 'services/dependencyResolverFor', 'services/routeResolver'], function (config, dependencyResolverFor) {
    var app = angular.module('BiletApp',
		[
			'ngRoute',
			'ngStorage',
			'routeResolverServices',
			'pascalprecht.translate',
			'ui.bootstrap',
			'chieffancypants.loadingBar',
			'pasvaz.bindonce'
		]);

    app.config(
        [
            '$routeProvider',
            '$locationProvider',
            '$controllerProvider',
            '$compileProvider',
            '$httpProvider',
            '$filterProvider',
            '$provide',
            '$translateProvider',
            'cfpLoadingBarProvider',
            function($routeProvider, $locationProvider, $controllerProvider, $compileProvider, $httpProvider, $filterProvider, $provide, $translateProvider, cfpLoadingBarProvider) {

                app.controller = $controllerProvider.register;
                app.directive = $compileProvider.directive;
                app.filter = $filterProvider.register;
                app.factory = $provide.factory;
                app.service = $provide.service;

                $httpProvider.defaults.useXDomain = true;
                $httpProvider.defaults.withCredentials = true;
                delete $httpProvider.defaults.headers.common["X-Requested-With"];

                cfpLoadingBarProvider.includeSpinner = false;
                cfpLoadingBarProvider.includeBar = true;

                var language = window.navigator.userLanguage || window.navigator.language;

                //Bootstrap URL Confiuration
                if (config.routes !== undefined) {
                    angular.forEach(config.routes, function(route, path) {
                        $routeProvider.when(path, { templateUrl: config.template + route.templateUrl, resolve: dependencyResolverFor(route.dependencies), data: route.data }).otherwise({ redirectTo: '/' });
                    });
                }
                if (config.defaultRoutePaths !== undefined) {
                    $routeProvider.otherwise({ redirectTo: config.defaultRoutePaths }).otherwise({ redirectTo: '/' });
                }

                
                //// use the HTML5 History API

                if (window.history && window.history.pushState) {
                    $locationProvider.html5Mode(true);
                }
                $locationProvider.html5Mode({
                    enabled: true,
                    requireBase: false
                });

            }
        ])
        .constant('API_URL', 'http://api.pirbet.info/');


    app.run(function ($rootScope) {

        $rootScope.$on("$routeChangeStart", function (event, next) {
            console.log("route changed");
        });


        $rootScope.$on('$viewContentLoading', function (event, viewConfig) {
            console.log("content is loaded");
        });

    });
     
    return app;
});