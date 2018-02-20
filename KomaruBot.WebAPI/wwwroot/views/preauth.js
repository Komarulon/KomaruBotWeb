'use strict';

angular.module('KomaruBot')

.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/preauth', {
        templateUrl: 'views/preauth.html',
        controller: 'PreAuthController'
    });
}])

.controller('PreAuthController', ['$scope', '$route', '$location', 'authService', function ($scope, $route, $location, authService) {
    $scope.isLoggedIn = authService.isLoggedIn();
    $scope.twitchAuthLink = null;

    if (!$scope.isLoggedIn) {
        window.sessionStorage.removeItem("username");
        window.sessionStorage.removeItem("accesstoken");

        $.PerformHttpRequest({
            type: "GET",
            url: "/api/auth/authendpoint",
            queryString: null,
            data: null,
            loadingIcon: null,
            error: null,
            success: function (json) {
                $scope.twitchAuthLink = json.authEndpoint;
                $scope.$apply();
            },
        });
    } else {

        // After getting the token, send a request to make sure it's valid:
        $.PerformHttpRequest({
            type: "GET",
            url: "api/auth/",
            queryString: null,
            data: null,
            loadingIcon: null,
            error: null,
            success: function (json) {
                if (json.resultString == "Success") {
                    window.sessionStorage["username"] = json.details.token.user_name;
                    $location.path('/main');
                    $scope.$apply();
                } else {
                    alert("There was an error logging in. Result was " + json.resultString + ". Message: " + json.message);
                }
            },
        });
    }
}]);