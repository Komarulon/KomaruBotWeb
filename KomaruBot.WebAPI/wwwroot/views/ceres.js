'use strict';

angular.module('KomaruBot')
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/ceres', {
        templateUrl: 'views/ceres.html',
        controller: 'CeresController'
    });
}])
.controller('CeresController', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
    $scope.isLoggedIn = authService.isLoggedIn();
    if (!$scope.isLoggedIn || (window.sessionStorage["username"] == null)) { $location.path('/preauth'); setTimeout(function () { location.reload(); }, 0); }


}]);