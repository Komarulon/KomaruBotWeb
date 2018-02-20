'use strict';

angular.module('KomaruBot', [
    'ngRoute',
    'ngMaterial'
])
.config(['$locationProvider', '$routeProvider', function ($locationProvider, $routeProvider) {
    $routeProvider.otherwise({ redirectTo: '/preauth' });
}]);
