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

    $scope.userID = window.sessionStorage["username"];
    $scope.ceresEnabled = false;

    $scope.loaded = false;
    $scope.loadingMessage = "Loading...";
    $scope.errorMessage = null;
    $scope.successMessage = null;


    $.PerformHttpRequest({
        type: "GET",
        url: "api/settings/ceres",
        queryString: null,
        data: null,
        loadingIcon: null,
        error: null,
        success: function (json) {
            $scope.ceresConfiguration = json.ceresConfiguration;

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
                $scope.errorMessage = "There was an error loading your Ceres settings. Please let Komaru know.";
                $scope.successMessage = null;
            }
            $scope.$apply();
        },
    });

    $scope.saveSettings = function () {
        var model = {
            userID: $scope.userID,
            ceresConfiguration: $scope.ceresConfiguration,
        };

        $scope.loadingMessage = "Saving Ceres settings...";
        $scope.errorMessage = null;
        $scope.successMessage = null;

        $.PerformHttpRequest({
            type: "PUT",
            url: "api/settings/ceres",
            queryString: null,
            data: model,
            loadingIcon: null,
            error: null,
            success: function (json) {

                $scope.loadingMessage = null;
                $scope.errorMessage = null;
                $scope.successMessage = "Ceres settings saved.";
                $scope.$apply();

                setTimeout(function () {
                    if ($scope.successMessage == "Ceres settings saved.") {
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
                    $scope.errorMessage = "There was an error saving your Ceres settings. Please let Komaru know.";
                    $scope.successMessage = null;
                }
                $scope.$apply();
            },
        });
    };

    $scope.anyNullClosestRewards = function (ceresConfig) {
        if (ceresConfig == null) { return true; }

        for (var i = 0; i < ceresConfig.closestRewards.length; i++) {
            var response = ceresConfig.closestRewards[i];

            if (response.rankAwarded === "" || 
                response.pointsAwarded === "" || response.pointsAwarded == 0) {
                return true;
            }
        }

        return false;
    };

    $scope.addClosestReward = function (ceresConfig) {
        var highestRank = 0;
        for (var i = 0; i < ceresConfig.closestRewards.length; i++) {
            var response = ceresConfig.closestRewards[i];
            if (response.rankAwarded > highestRank) {
                highestRank = response.rankAwarded;
            }   
        }

        ceresConfig.closestRewards.push({ rankAwarded: highestRank + 1, pointsAwarded: 0, awardEvenIfOtherWinners: false, });
    };

    $scope.removeClosestReward = function (ceresConfig, idx) {
        ceresConfig.closestRewards.splice(idx, 1);
    };




    $scope.anyNullStaticRewards = function (ceresConfig) {
        if (ceresConfig == null) { return true; }

        for (var i = 0; i < ceresConfig.staticRewards.length; i++) {
            var response = ceresConfig.staticRewards[i];

            if (response.hundrethsLeewayStart === "" ||
                response.hundrethsLeewayEnd === "" ||
                response.pointsAwarded === "" || response.pointsAwarded == 0) {
                return true;
            }
        }

        return false;
    };

    $scope.addStaticReward = function (ceresConfig) {
        var highestLeewayEnd = -1;
        for (var i = 0; i < ceresConfig.staticRewards.length; i++) {
            var response = ceresConfig.staticRewards[i];
            if (response.hundrethsLeewayEnd > highestLeewayEnd) {
                highestLeewayEnd = response.hundrethsLeewayEnd;
            }
        }

        ceresConfig.staticRewards.push({ hundrethsLeewayStart: highestLeewayEnd + 1, hundrethsLeewayEnd: highestLeewayEnd + 1, pointsAwarded: 0, });
    };

    $scope.removeStaticReward = function (ceresConfig, idx) {
        ceresConfig.staticRewards.splice(idx, 1);
    };

    $scope.fixStaticRewardRange = function (reward) {
        if (reward.hundrethsLeewayStart < 0) {
            reward.hundrethsLeewayStart = 0;
        }
        if (reward.hundrethsLeewayEnd < 0) {
            reward.hundrethsLeewayEnd = 0;
        }
        if (reward.hundrethsLeewayEnd < reward.hundrethsLeewayStart) {
            reward.hundrethsLeewayEnd = reward.hundrethsLeewayStart;
        }
    };


    $scope.anyNullMagicTimes = function (ceresConfig) {
        if (ceresConfig == null) { return true; }

        for (var i = 0; i < ceresConfig.magicTimes.length; i++) {
            var response = ceresConfig.magicTimes[i];

            if (response.ceresTime === "" ||
                response.pointsAwarded === "" || response.pointsAwarded == 0) {
                return true;
            }
        }

        return false;
    };

    $scope.addMagicTime = function (ceresConfig) {
        ceresConfig.magicTimes.push({ ceresTime: 4700, pointsAwarded: 0, });
    };

    $scope.removeMagicTime = function (ceresConfig, idx) {
        ceresConfig.magicTimes.splice(idx, 1);
    };
}]);