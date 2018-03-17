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
    $scope.gambleConfiguration = null;

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
            $scope.gambleConfiguration = json.gambleConfiguration;

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
            gambleConfiguration: $scope.gambleConfiguration
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

    $scope.getAmountAwardedForRoll = function (multiplier, amount) {
        if (multiplier == 1) {
            return amount;
        }

        if (multiplier < 1) {
            var amountLost = parseInt(((1 - multiplier) * amount), 10);
            return amountLost * -1;
        } else {
            var amountGained = parseInt((multiplier * amount), 10);
            return amountGained;
        }
    };

    $scope.getTotalAmountOfMoneyWon = function (gambleConfig) {
        if (gambleConfig == null) {
            return 0;
        }

        var totalWon = 0;
        for (var i = 0; i < gambleConfig.rollResults.length; i++) {
            var rollResult = gambleConfig.rollResults[i];
            totalWon += $scope.getAmountAwardedForRoll(rollResult.multiplier, 100);
        }

        return totalWon
    }
}]);