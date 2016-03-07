'use strict';

define(['app'], function (app) {

 
	app.directive('validSubmit', [
		'$parse', function ($parse) {
			return {
				// we need a form controller to be on the same element as this directive
				// in other words: this directive can only be used on a &lt;form&gt;
				require: 'form',
				// one time action per form
				link: function (scope, element, iAttrs, form) {
					form.$submitted = false;
					// get a hold of the function that handles submission when form is valid
					var fn = $parse(iAttrs.validSubmit);

					// register DOM event handler and wire into Angular's lifecycle with scope.$apply
					element.on('submit', function (event) {
						scope.$apply(function () {
							// on submit event, set submitted to true (like the previous trick)
							form.$submitted = true;
							// if form is valid, execute the submission handler function and reset form submission state
							if (form.$valid) {
								fn(scope, { $event: event });
								form.$submitted = false;
							}
						});
					});
				}
			};
		}
	]);


	app.directive('ngEnter', function () {
		return function (scope, element, attrs) {
			element.bind("keydown keypress", function (event) {
				if (event.which === 13) {
					scope.$apply(function () {
						scope.$eval(attrs.ngEnter, { 'event': event });
					});

					event.preventDefault();
				}
			});
		};
	});


	app.directive('numbersOnly', function () {
		return {
			require: 'ngModel',
			link: function (scope, element, attrs, modelCtrl) {
				modelCtrl.$parsers.push(function (inputValue) {
					// this next if is necessary for when using ng-required on your input. 
					// In such cases, when a letter is typed first, this parser will be called
					// again, and the 2nd time, the value will be undefined
					if (inputValue == undefined) return ''
					var transformedInput = inputValue.replace(/[^0-9]/g, '');
					if (transformedInput != inputValue) {
						modelCtrl.$setViewValue(transformedInput);
						modelCtrl.$render();
					}
					return transformedInput;
				});
			}
		};
	});

	app.directive('checkEmail', ['$http', 'API_URL', 'Params', function ($http, API_URL, Params) {
		return {
			require: 'ngModel',
			link: function (scope, element, attrs, c) {
				scope.$watch(attrs.ngModel, function (val) {

				});
				element.bind('blur', function () {
					$http({
						method: 'POST',
						url: API_URL + Params.Account.CheckEmail,
						params: { 'Email': c.$viewValue }
					})
					.success(function (data, status, headers, cfg) {
						if (!data.Data) {
							c.$setValidity('required', true);
						}
					})
					.error(function (data, status, headers, cfg) {
					});
				});

			}
		}
	}]);


	app.directive('counter', function () {
		return {
			restrict: 'A',
			scope: { value: '=value' },
			template: '<input type="text" class="counter-field" ng-model="value" ng-change="changed()" ng-readonly="readonly">\
				<a href="javascript:;" class="counter-plus" ng-click="plus()">+</a>\
				<a href="javascript:;" class="counter-minus" ng-click="minus()">-</a>',
			link: function (scope, element, attributes) {
				// Make sure the value attribute is not missing.
				if (angular.isUndefined(scope.value)) {
					throw "Missing the value attribute on the counter directive.";
				}

				var min = angular.isUndefined(attributes.min) ? null : parseInt(attributes.min);
				var max = angular.isUndefined(attributes.max) ? null : parseInt(attributes.max);
				var step = angular.isUndefined(attributes.step) ? 1 : parseInt(attributes.step);

				element.addClass('counter-container');

				// If the 'editable' attribute is set, we will make the field editable.
				scope.readonly = angular.isUndefined(attributes.editable) ? true : false;

				/**
				 * Sets the value as an integer.
				 */
				var setValue = function (val) {
					scope.value = parseInt(val);
				}

				// Set the value initially, as an integer.
				setValue(scope.value);

				/**
				 * Decrement the value and make sure we stay within the limits, if defined.
				 */
				scope.minus = function () {
					if (min && (scope.value <= min || scope.value - step <= min) || min === 0 && scope.value < 1) {
						setValue(min);
						return false;
					}
					setValue(scope.value - step);
				};

				/**
				 * Increment the value and make sure we stay within the limits, if defined.
				 */
				scope.plus = function () {
					if (max && (scope.value >= max || scope.value + step >= max)) {
						setValue(max);
						return false;
					}
					setValue(scope.value + step);
				};

				/**
				 * This is only triggered when the field is manually edited by the user.
				 * Where we can perform some validation and make sure that they enter the
				 * correct values from within the restrictions.
				 */
				scope.changed = function () {
					// If the user decides to delete the number, we will set it to 0.
					if (!scope.value) setValue(0);

					// Check if what's typed is numeric or if it has any letters.
					if (/[0-9]/.test(scope.value)) {
						setValue(scope.value);
					}
					else {
						setValue(scope.min);
					}

					// If a minimum is set, let's make sure we're within the limit.
					if (min && (scope.value <= min || scope.value - step <= min)) {
						setValue(min);
						return false;
					}

					// If a maximum is set, let's make sure we're within the limit.
					if (max && (scope.value >= max || scope.value + step >= max)) {
						setValue(max);
						return false;
					}

					// Re-set the value as an integer.
					setValue(scope.value);
				};
			}
		}
	});


	app.run(function ($rootScope) {
		angular.element(document).on("click", function (e) {
			$rootScope.$broadcast("documentClicked", angular.element(e.target));
		});
	});


    /**
 * Angucomplete
 * Autocomplete directive for AngularJS
 * By Daryl Rowland
 */

	app.directive('angucomplete', function ($parse, $http) {
            return {
                restrict: 'EA',
                scope: {
                    "id": "@id",
                    "placeholder": "@placeholder",
                    "selectedObject": "=selectedobject",
                    "url": "@url",
                    "titleField": "@titlefield",
                    "descriptionField": "@descriptionfield",
                    "imageField": "@imagefield",
                    "inputClass": "@inputclass",
                    "userPause": "@pause",
                    "localData": "=localdata",
                    "searchFields": "@searchfields",
                    "minLengthUser": "@minlength"
                },
                template: '<div class="angucomplete-holder"><input id="{{id}}_value" ng-model="searchStr" type="text" placeholder="{{placeholder}}" class="{{inputClass}}" ng-keyup="keyPressed($event)"/><div id="{{id}}_dropdown" class="angucomplete-dropdown" ng-if="showDropdown"><div class="angucomplete-searching" ng-show="searching">Searching...</div><div class="angucomplete-searching" ng-show="!searching && (!results || results.length == 0)">No results found</div><div class="angucomplete-row" ng-repeat="result in results" ng-click="selectResult(result)" ng-mouseover="hoverRow()" ng-class="{\'angucomplete-selected-row\': $index == currentIndex}"><div ng-if="result.image && result.image != \'\'" class="angucomplete-image-holder"><img ng-src="{{result.image}}" class="angucomplete-image"/></div><div>{{result.title}}</div><div ng-if="result.description && result.description != \'\'" class="angucomplete-description">{{result.description}}</div></div></div></div>',
                controller: function ($scope) {
                    $scope.lastFoundWord = null;
                    $scope.currentIndex = null;
                    $scope.justChanged = false;
                    $scope.searchTimer = null;
                    $scope.searching = false;
                    $scope.pause = 500;
                    $scope.minLength = 3;

                    if ($scope.minLengthUser && $scope.minLengthUser != "") {
                        $scope.minLength = $scope.minLengthUser;
                    }

                    if ($scope.userPause) {
                        $scope.pause = $scope.userPause;
                    }

                    $scope.processResults = function (responseData) {
                        if (responseData && responseData.length > 0) {
                            $scope.results = [];

                            var titleFields = [];
                            if ($scope.titleField && $scope.titleField != "") {
                                titleFields = $scope.titleField.split(",");
                            }

                            for (var i = 0; i < responseData.length; i++) {
                                // Get title variables
                                var titleCode = "";

                                for (var t = 0; t < titleFields.length; t++) {
                                    if (t > 0) {
                                        titleCode = titleCode + " + ' ' + ";
                                    }
                                    titleCode = titleCode + "responseData[i]." + titleFields[t];
                                }

                                // Figure out description
                                var description = "";

                                if ($scope.descriptionField && $scope.descriptionField != "") {
                                    eval("description = responseData[i]." + $scope.descriptionField);
                                }

                                // Figure out image
                                var image = "";

                                if ($scope.imageField && $scope.imageField != "") {
                                    eval("image = responseData[i]." + $scope.imageField);
                                }

                                var resultRow = {
                                    title: eval(titleCode),
                                    description: description,
                                    image: image,
                                    originalObject: responseData[i]
                                }

                                $scope.results[$scope.results.length] = resultRow;
                            }


                        } else {
                            $scope.results = [];
                        }
                    }

                    $scope.searchTimerComplete = function (str) {
                        // Begin the search

                        if (str.length >= $scope.minLength) {
                            if ($scope.localData) {
                                var searchFields = $scope.searchFields.split(",");

                                var matches = [];
                                
                                for (var i = 0; i < $scope.localData.length; i++) {
                                    var match = false;

                                    for (var s = 0; s < searchFields.length; s++) {
                                        var evalStr = 'match = match || ($scope.localData[i].' + searchFields[s] + '.toLowerCase().indexOf("' + str.toLowerCase() + '") >= 0)';
                                        eval(evalStr);
                                    }

                                    if (match) {
                                        matches[matches.length] = $scope.localData[i];
                                    }
                                }

                                $scope.searching = false;
                                $scope.processResults(matches);
                                $scope.$apply();


                            } else {
                                $http.get($scope.url + str, {}).
                                success(function (responseData, status, headers, config) {
                                    $scope.searching = false;
                                    $scope.processResults(responseData);
                                }).
                                error(function (data, status, headers, config) {
                                    console.log("error");
                                });
                            }
                        }

                    }

                    $scope.hoverRow = function (index) {
                        $scope.currentIndex = index;
                    }

                    $scope.keyPressed = function (event) {
                        if (!(event.which == 38 || event.which == 40 || event.which == 13)) {
                            if (!$scope.searchStr || $scope.searchStr == "") {
                                $scope.showDropdown = false;
                            } else {

                                if ($scope.searchStr.length >= $scope.minLength) {
                                    $scope.showDropdown = true;
                                    $scope.currentIndex = -1;
                                    $scope.results = [];

                                    if ($scope.searchTimer) {
                                        clearTimeout($scope.searchTimer);
                                    }

                                    $scope.searching = true;

                                    $scope.searchTimer = setTimeout(function () {
                                        $scope.searchTimerComplete($scope.searchStr);
                                    }, $scope.pause);
                                }


                            }

                        } else {
                            event.preventDefault();
                        }
                    }

                    $scope.selectResult = function (result) {
                        $scope.searchStr = result.title;
                        $scope.selectedObject = result;
                        $scope.showDropdown = false;
                        $scope.results = [];
                        //$scope.$apply();
                    }
                },

                link: function ($scope, elem, attrs, ctrl) {

                    elem.bind("keyup", function (event) {
                        if (event.which === 40) {
                            if (($scope.currentIndex + 1) < $scope.results.length) {
                                $scope.currentIndex++;
                                $scope.$apply();
                                event.preventDefault;
                                event.stopPropagation();
                            }

                            $scope.$apply();
                        } else if (event.which == 38) {
                            if ($scope.currentIndex >= 1) {
                                $scope.currentIndex--;
                                $scope.$apply();
                                event.preventDefault;
                                event.stopPropagation();
                            }

                        } else if (event.which == 13) {
                            if ($scope.currentIndex >= 0 && $scope.currentIndex < $scope.results.length) {
                                $scope.selectResult($scope.results[$scope.currentIndex]);
                                $scope.$apply();
                                event.preventDefault;
                                event.stopPropagation();
                            } else {
                                $scope.results = [];
                                $scope.$apply();
                                event.preventDefault;
                                event.stopPropagation();
                            }

                        } else if (event.which == 27) {
                            $scope.results = [];
                            $scope.showDropdown = false;
                            $scope.$apply();
                        } else if (event.which == 8) {
                            $scope.selectedObject = null;
                            $scope.$apply();
                        }
                    });


                }
            };
        });

});
