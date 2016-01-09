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


	app.directive("dropdown", function ($rootScope) {
		return {
			restrict: "E",
			template: '<div class="dropdown-container" ng-class="{ show: listVisible }"><div class="dropdown-display" ng-click="show();" ng-class="{ clicked: listVisible }"><span ng-if="!isPlaceholder">{{display}}</span><span class="placeholder" ng-if="isPlaceholder">{{placeholder}}lt;/span><i class="fa fa-angle-down"></i></div><div class="dropdown-list"><div><div bindonce ng-repeat="item in list" ng-click="select(item)" ng-class="{ selected: isSelected(item) }"><span>{{property !== undefined ? item[property] : item}}</span><i class="fa fa-check"></i></div></div></div></div>',
			scope: {
				placeholder: "@",
				list: "=",
				selected: "=",
				property: "@"
			},
			link: function (scope) {
				scope.listVisible = false;
				scope.isPlaceholder = true;

				scope.select = function (item) {
					scope.isPlaceholder = false;
					scope.selected = item;
				};

				scope.isSelected = function (item) {
					return item[scope.property] === scope.selected[scope.property];
				};

				scope.show = function () {
					scope.listVisible = true;
				};

				$rootScope.$on("documentClicked", function (inner, target) {
					console.log($(target[0]).is(".dropdown-display.clicked") || $(target[0]).parents(".dropdown-display.clicked").length > 0);
					if (!$(target[0]).is(".dropdown-display.clicked") && !$(target[0]).parents(".dropdown-display.clicked").length > 0)
						scope.$apply(function () {
							scope.listVisible = false;
						});
				});

				scope.$watch("selected", function (value) {
					scope.isPlaceholder = scope.selected[scope.property] === undefined;
					scope.display = scope.selected[scope.property];
				});
			}
		}
	});


	app.directive('bsDropdown', function ($compile) {
		return {
			restrict: 'E',
			scope: {
				items: '=dropdownData',
				doSelect: '&selectVal',
				selectedItem: '=preselectedItem',
				defaultText: '@'
			},
			link: function (scope, element, attrs) {
				var html = '';
				switch (attrs.menuType) {
					case "button":
						html += '<div class="btn-group"><button class="btn button-label btn-info">Action</button><button class="btn btn-info dropdown-toggle" data-toggle="dropdown"> <span class="m-d-arrow"></span></button>';
						break;
					default:
						html += '<div class="dropdown"><a class="dropdown-toggle" role="button" data-toggle="dropdown"  href="javascript:;"> <span class="default text">' + ((typeof scope.defaultText != 'undefined') ? scope.defaultText : 'Seçiniz') + '</span> <span class="m-d-arrow"></span></a>';
						break;
				}
				html += '<ul class="dropdown-menu"><li bindonce ng-repeat="item in items"><a tabindex="-1" data-ng-click="selectVal(item)">{{item.name}}</a></li></ul></div>';
				element.append($compile(html)(scope));
				for (var i = 0; i < scope.items.length; i++) {
					if (scope.items[i].id === scope.selectedItem) {
						scope.bSelectedItem = scope.items[i];
						break;
					}
				}
				scope.selectVal = function (item) {
					if (typeof item == 'undefined')
						return false;

					switch (attrs.menuType) {
						case "button":
							$('button.button-label', element).html(item.name);
							break;
						default:
							$('a.dropdown-toggle', element).html('<span class="name">' + item.name + '</span> <span class="m-d-arrow"></span>');
							break;
					}
					scope.doSelect({
						selectedVal: item.id
					});
				};
				scope.selectVal(scope.bSelectedItem);
			}
		};
	});



	app.directive('slideable', function () {
		return {
			restrict: 'C',
			compile: function (element, attr) {
				// wrap tag
				var contents = element.html();
				element.html('<div class="slideable_content" style="margin:0 !important; padding:0 !important" >' + contents + '</div>');

				return function postLink(scope, element, attrs) {
					// default properties
					attrs.duration = (!attrs.duration) ? '1s' : attrs.duration;
					attrs.easing = (!attrs.easing) ? 'ease-in-out' : attrs.easing;
					element.css({
						'overflow': 'hidden',
						'height': '0px',
						'transitionProperty': 'height',
						'transitionDuration': attrs.duration,
						'transitionTimingFunction': attrs.easing
					});
				};
			}
		};
	})
	.directive('slideToggle', function () {
		return {
			restrict: 'A',
			link: function (scope, element, attrs) {
				var target, content;

				attrs.expanded = false;

				element.bind('click', function () {
					if (!target) target = document.querySelector(attrs.slideToggle);
					if (!content) content = target.querySelector('.slideable_content');

					if (!attrs.expanded) {
						content.style.border = '1px solid rgba(0,0,0,0)';
						var y = content.clientHeight;
						content.style.border = 0;
						target.style.height = y + 'px';
					} else {
						target.style.height = '0px';
					}
					attrs.expanded = !attrs.expanded;
				});
			}
		}
	});



	app.directive('phoneInput', function($filter, $browser) {
		return {
			require: 'ngModel',
			link: function($scope, $element, $attrs, ngModelCtrl) {
				var listener = function() {
					var value = $element.val().replace(/[^0-9]/g, '');
					$element.val($filter('tel')(value, false));
				};

				// This runs when we update the text field
				ngModelCtrl.$parsers.push(function(viewValue) {
					return viewValue.replace(/[^0-9]/g, '').slice(0,10);
				});

				// This runs when the model gets updated on the scope directly and keeps our view in sync
				ngModelCtrl.$render = function() {
					$element.val($filter('tel')(ngModelCtrl.$viewValue, false));
				};

				$element.bind('change', listener);
				$element.bind('keydown', function(event) {
					var key = event.keyCode;
					// If the keys include the CTRL, SHIFT, ALT, or META keys, or the arrow keys, do nothing.
					// This lets us support copy and paste too
					if (key == 91 || (15 < key && key < 19) || (37 <= key && key <= 40)){
						return;
					}
					$browser.defer(listener); // Have to do this or changes don't get picked up properly
				});

				$element.bind('paste cut', function() {
					$browser.defer(listener);
				});
			}

		};
	});
	app.filter('tel', function () {
		return function (tel) {
			console.log(tel);
			if (!tel) { return ''; }

			var value = tel.toString().trim().replace(/^\+/, '');

			if (value.match(/[^0-9]/)) {
				return tel;
			}

			var country, city, number;

			switch (value.length) {
				case 1:
				case 2:
				case 3:
					city = value;
					break;

				default:
					city = value.slice(0, 3);
					number = value.slice(3);
			}

			if(number){
				if(number.length>3){
					number = number.slice(0, 3) + '-' + number.slice(3,7);
				}
				else{
					number = number;
				}

				return ("(" + city + ") " + number).trim();
			}
			else{
				return "(" + city;
			}

		};
	});


});
