'use strict';

angular.module('KomaruBot')
.controller('navBarController', ['$scope', '$location', '$rootScope', 'authService', function ($scope, $location, $rootScope, authService) {

    $scope.isLoggedIn = authService.isLoggedIn();
    $scope.currentNavItem = $location.path().replace(/^\//, '');

    $scope.goto = function (page) {
        if (page != $scope.currentNavItem) {
            $location.path('/' + page);
        }
    }

    $scope.username = window.sessionStorage["username"];
    $scope.logOut = function () {
        window.sessionStorage.removeItem("username");
        window.sessionStorage.removeItem("accesstoken");
        $location.path('/preauth');
        setTimeout(function () { location.reload(); }, 0); 
    }
}]);