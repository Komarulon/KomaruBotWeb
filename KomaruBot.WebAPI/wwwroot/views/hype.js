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

    $scope.userID = window.sessionStorage["username"];

    $scope.loaded = false;
    $scope.loadingMessage = "Loading...";
    $scope.errorMessage = null;
    $scope.successMessage = null;

    $scope.idct = 0;

    $.PerformHttpRequest({
        type: "GET",
        url: "api/settings/hype",
        queryString: null,
        data: null,
        loadingIcon: null,
        error: null,
        success: function (json) {
            $scope.userID = json.userID;
            $scope.hypeCommands = json.hypeCommands;

            for (var i = 0; i < $scope.hypeCommands.length; i++) {
                var hc = $scope.hypeCommands[i];
                hc.id = "HC_" + $scope.idct++;
            }

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
                $scope.errorMessage = "There was an error loading your Hype Commands. Please let Komaru know.";
                $scope.successMessage = null;
            }
            $scope.$apply();
        },
    });

    $scope.saveSettings = function () {
        var model = {
            userID: $scope.userID,
            hypeCommands: $scope.hypeCommands,
        };

        $scope.loadingMessage = "Saving Hype commands...";
        $scope.errorMessage = null;
        $scope.successMessage = null;

        $.PerformHttpRequest({
            type: "PUT",
            url: "api/settings/hype",
            queryString: null,
            data: model,
            loadingIcon: null,
            error: null,
            success: function (json) {

                $scope.loadingMessage = null;
                $scope.errorMessage = null;
                $scope.successMessage = "Hype Commands saved.";
                $scope.$apply();

                setTimeout(function () {
                    if ($scope.successMessage == "Hype Commands saved.") {
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
                    $scope.errorMessage = "There was an error saving your Hype Commands. Please let Komaru know.";
                    $scope.successMessage = null;
                }
                $scope.$apply();
            },
        });
    };

    $scope.anyNullResponses = function (hypeCommand) {
        for (var i = 0; i < hypeCommand.commandResponses.length; i++) {
            var response = hypeCommand.commandResponses[i];
            if (response.message == "" || response.message == null) {
                return true;
            }
        }

        return false;
    };

    $scope.addResponse = function (hypeCommand) {
        hypeCommand.commandResponses.push({ message: null });
    };

    $scope.removeResponse = function (hypeCommand, idx) {
        hypeCommand.commandResponses.splice(idx, 1);
    };

    $scope.addHypeCommand = function () {

        $scope.hypeCommands.push({
            id: "HC_" + $scope.idct++,
            ShowHide: true,

            enabled: true,
            pointsCost: 0,
            accessLevel: 0,
            commandText: "!",
            commandResponses: [{ message: null }],
            randomizeResponseOrders: true, 
            numberOfResponses: 1,
        });
    };

    $scope.accessLevels = [
        { 'id': 0, 'label': "Public" },
        { 'id': 1, 'label': "Moderator and Broadcaster only" },
        { 'id': 2, 'label': "Broadcaster only" },
    ];
}]);