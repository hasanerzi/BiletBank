"use strict";

require.config({
	baseUrl: '/App/scripts',
    //urlArgs: "v=" + (new Date()).getTime(),
	urlArgs: "bust=" + (new Date()).getTime(),
	waitSeconds: 0,
	paths: {
		'angular'								: '/bower_components/angular/angular.min',
		'angular-route'							: '/bower_components/angular-route/angular-route.min',
		'angular-storage'                       : '/bower_components/ngStorage/ngStorage.min',
		'angular-sanitize'						: '/bower_components/angular-sanitize/angular-sanitize.min',

		//Language Loader
		'angular-translate'						: '/bower_components/angular-translate/angular-translate.min',
		'angular-translate-loader-url'			: '/bower_components/angular-translate-loader-url/angular-translate-loader-url.min',
		'angular-translate-loader-static-files'	: '/bower_components/angular-translate-loader-static-files/angular-translate-loader-static-files.min',

		//Bootstrap
		'ui.bootstrap'							: '/bower_components/angular-bootstrap/ui-bootstrap-tpls',

		//Loading Bar
		'angular-loading-bar'					: '/bower_components/angular-loading-bar/src/loading-bar',
		'angular-svg-round-progress'            : '/bower_components/angular-svg-round-progressbar/build/roundProgress.min',

	    //Timer
		'angular-timer'					        : '/bower_components/angular-timer/dist/angular-timer.min',

		//not used....
		'angular-bindonce'                      : '/bower_components/angular-bindonce/bindonce'

	},
	shim: {
		'app': {
			deps: [
				'angular',
				'angular-route',
				'ui.bootstrap',
				'angular-translate',
				'angular-storage',
				'angular-loading-bar',
				'angular-sanitize',
				'angular-bindonce'
			]
		},
		'angular-route'								: ['angular'],
		'angular-sanitize'							: ['angular'],
		'ui.bootstrap'								: ['angular'],
		'angular-translate'							: ['angular'],
		'angular-storage'							: ['angular'],
		'angular-loading-bar'						: ['angular'],
		'angular-timer'						        : ['angular'],
		'angular-bindonce'							: ['angular']
 
	}

});

require
(
	[
		'app',
		'controllers/MainController'
 	],
	function (app) {
		angular.bootstrap(document, ['BiletApp']);
	}
);