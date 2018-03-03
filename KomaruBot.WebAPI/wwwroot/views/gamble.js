'use strict';

angular.module('KomaruBot')
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/gamble', {
        templateUrl: 'views/gamble.html',
        controller: 'GambleController'
    });
}])
.controller('GambleController', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
    $scope.isLoggedIn = authService.isLoggedIn();
    if (!$scope.isLoggedIn || (window.sessionStorage["username"] == null)) { $location.path('/preauth'); setTimeout(function () { location.reload(); }, 0); }

    $scope.userID = window.sessionStorage["username"];
    $scope.gambleEnabled = false;
    $scope.minBid = 1;
    $scope.maxBid = 1;
    $scope.minMinutesBetweenGambles = 0;
    $scope.rollResults = [];

    $scope.loaded = false;
    $scope.loadingMessage = "Loading...";
    $scope.errorMessage = null;
    $scope.successMessage = null;


    $.PerformHttpRequest({
        type: "GET",
        url: "api/settings/gamble",
        queryString: null,
        data: null,
        loadingIcon: null,
        error: null,
        success: function (json) {
            $scope.gambleEnabled = json.gambleConfiguration.gambleEnabled;
            $scope.minBid = json.gambleConfiguration.minBid;
            $scope.maxBid = json.gambleConfiguration.maxBid;
            $scope.minMinutesBetweenGambles = json.gambleConfiguration.minMinutesBetweenGambles;
            $scope.rollResults = json.gambleConfiguration.rollResults;

            $scope.loaded = true;
            $scope.loadingMessage = null;
            $scope.errorMessage = null;
            $scope.successMessage = null;
            $scope.$apply();
        },
        error: function (response) {
            if (response != null && response.responseJSON != null && response.responseJSON.message != null) {
                $scope.loadingMessage = null;
                $scope.errorMessage = response.responseJSON.message;
                $scope.successMessage = null;
            } else {
                $scope.loadingMessage = null;
                $scope.errorMessage = "There was an error loading your Gamble settings. Please let Komaru know.";
                $scope.successMessage = null;
            }
            $scope.$apply();
        },
    });

    $scope.saveSettings = function () {
        var model = {
            userID: $scope.userID,
            gambleConfiguration: {
                gambleEnabled: $scope.gambleEnabled,
                minBid: $scope.minBid,
                maxBid: $scope.maxBid,
                minMinutesBetweenGambles: $scope.minMinutesBetweenGambles,
                rollResults: $scope.rollResults,
            }
        };

        $scope.loadingMessage = "Saving Gamble settings...";
        $scope.errorMessage = null;
        $scope.successMessage = null;

        $.PerformHttpRequest({
            type: "PUT",
            url: "api/settings/gamble",
            queryString: null,
            data: model,
            loadingIcon: null,
            error: null,
            success: function (json) {

                $scope.loadingMessage = null;
                $scope.errorMessage = null;
                $scope.successMessage = "Gamble settings saved.";
                $scope.$apply();

                setTimeout(function () {
                    if ($scope.successMessage == "Gamble settings saved.") {
                        $scope.successMessage = null;
                        $scope.$apply();
                    }
                }, 10000);
            },
            error: function (response) {
                if (response != null && response.responseJSON != null && response.responseJSON.message != null) {
                    $scope.loadingMessage = null;
                    $scope.errorMessage = response.responseJSON.message;
                    $scope.successMessage = null;
                } else {
                    $scope.loadingMessage = null;
                    $scope.errorMessage = "There was an error saving your Gamble settings. Please let Komaru know.";
                    $scope.successMessage = null;
                }
                $scope.$apply();
            },
        });
    };

}]);