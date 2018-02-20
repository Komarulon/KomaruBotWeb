'use strict';

angular.module('KomaruBot')
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/hype', {
        templateUrl: 'views/hype.html',
        controller: 'HypeController'
    });
}])
.controller('HypeController', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
    $scope.isLoggedIn = authService.isLoggedIn();
    if (!$scope.isLoggedIn || (window.sessionStorage["username"] == null)) { $location.path('/preauth'); setTimeout(function () { location.reload(); }, 0); }

    
}]);