define([], function () {
    
	return {
	    defaultRoutePath: '/',
	    template: '/App/views/',
		routes: {
			'/': {
			    templateUrl: 'home/index.html',
				dependencies: [
					'controllers/HomeController'
				]
			},
			'/Error': {
				templateUrl: 'error.html'
			},
			'/Sport/:sportname/:sid/:categoryname/:cid': {
				templateUrl: 'Category/index.html',
				dependencies: [
					'controllers/CategoryController',
					'services/matchService'
				]
			}
		}
	};
});