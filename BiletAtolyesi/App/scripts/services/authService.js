'use strict';
//'services/sessionService'
define(['app'], function (app) {

    var authServiceFactory = function ($rootScope, $http, $q, $filter, $localStorage, Session, API_URL, Params) {
        var authService = {};
        var $translate = $filter('translate');

        authService.login = function (user) {
            var deferred = $q.defer();
            $http({
                method: 'POST',
                url: API_URL + Params.Account.LoginURL,
                params: {
                    Username: user.Username,
                    Password: user.Password
                }
            }).success(function (res) {
                deferred.resolve(res);
                if (typeof res.data === 'undefined') {
                    return res;
                };
                var info = Session.create(res.data.Id, res.data.FullName, res.data.LastLogin, 'registeredUser');
                $localStorage.userInfo = info;
            }).error(function (msg, code) {
                deferred.reject(msg);
                sweetAlert(code, msg, "error");
            });
            return deferred.promise;
        };


        authService.isAuthenticated = function () {
            if (typeof $localStorage.userInfo === 'undefined') {
                return false;
            }
            return !!$localStorage.userInfo.fullname;
        };

        authService.isAuthorized = function (authorizedRoles) {
            if (!angular.isArray(authorizedRoles)) {
                authorizedRoles = [authorizedRoles];
            }
            return (authService.isAuthenticated() &&
			  authorizedRoles.indexOf(Session.userInfo.userRole) !== -1);
        };

        authService.SetCredentials = function () {
            var deferred = $q.defer();
            console.log("set credential");
            $http({
                method: 'GET',
                url: API_URL + Params.Account.UserInfoURL,
                headers: {
                    'accept': 'true'
                }
            }).then(function (user) {

                if (user.data.Status === "Failed") {
                    if ($rootScope.isAuth) {
                        sweetAlert("", $translate('f.M0003'), "error");
                    }
                    authService.DeleteCredentials(); return false;
                }
                var userinfo = user.data.Data;
                console.log(userinfo);
                $rootScope.isAuth = true;
                $rootScope.userInfo = userinfo;
                Session.setUserInfo({
                    fullname: userinfo.FullName,
                    lastlogin: userinfo.LastLogin,
                    userRole: 'registered'
                });
            },
			function (msg, code) {
			    authService.DeleteCredentials();
			    deferred.reject(msg);
			});
            return deferred.promise;
        };

        authService.DeleteCredentials = function () {
            Session.destroy();
            $rootScope.isAuth = false;
            $rootScope.userInfo = null;
            delete $localStorage.userInfo;
        }

        authService.logout = function () {
            var deferred = $q.defer();
            $http({
                method: 'POST',
                url: API_URL + Params.Account.LogOutURL
            }).success(function (data) {
                deferred.resolve(data);
            }).error(function (msg, code) {
                deferred.reject(msg);
            });
            return deferred.promise;
        };


        authService.registerUser = function (user) {
            var deferred = $q.defer();
            $http({
                method: 'POST',
                url: API_URL + Params.Account.RegisterURL,
                params: user
            })
			.success(function (data) {
			    deferred.resolve(data);
			}).error(function (msg, code) {
			    deferred.reject(msg);
			    console.log("Register Error", msg, code);
			});
            return deferred.promise;
        }

        authService.getCountries = function () {
            var deferred = $q.defer();
            $http.get(API_URL + '/Countries')
			.success(function (data) {
			    deferred.resolve(data);
			}).error(function (msg, code) {
			    deferred.reject(msg);
			    $log.error(msg, code);
			});
            return deferred.promise;
        }
        return authService;
    };


    authServiceFactory.$inject = ['$rootScope', '$http', '$q', '$filter', '$localStorage', 'Session', 'API_URL', 'Params'];
    app.factory('authService', authServiceFactory);

});