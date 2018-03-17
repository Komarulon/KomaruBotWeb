'use strict';

angular.module('KomaruBot')
.controller('navBarController', ['$scope', '$location', '$rootScope', '$sce', 'authService', function ($scope, $location, $rootScope, $sce, authService) {

    $scope.isLoggedIn = authService.isLoggedIn();
    $scope.currentNavItem = $location.path().replace(/^\//, '');

    $scope.goto = function (page) {
        if (page != $scope.currentNavItem) {
            $location.path('/' + page);
        }
    }

    $scope.username = window.sessionStorage["username"];

    $scope.chatSrc = null;
    if ($scope.isLoggedIn) {
        
        $scope.chatSrc = $sce.trustAsResourceUrl("http://www.twitch.tv/embed/" + $scope.username + "/chat");
    }

    $scope.logOut = function () {
        console.log("ASDF");
        window.sessionStorage.removeItem("username");
        window.sessionStorage.removeItem("accesstoken");
        $location.path('/preauth');
        setTimeout(function () { location.reload(); }, 0); 
    }
}]);