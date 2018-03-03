﻿'use strict';

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
    $scope.errorMessage = null;
    $scope.successMessage = null;


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

            $scope.loaded = true;
            $scope.errorMessage = null;
            $scope.$apply();
        },
        error: function () {
            $scope.errorMessage = "There was an error loading your Hype Commands. Please let Komaru know.";
            $scope.successMessage = null;
            $scope.$apply();
        },
    });

    $scope.saveSettings = function () {
        var model = {
            userID: $scope.userID,
            //hypeCommands: 
        };

        $.PerformHttpRequest({
            type: "PUT",
            url: "api/settings/hype",
            queryString: null,
            data: model,
            loadingIcon: null,
            error: null,
            success: function (json) {
                $scope.successMessage = "Hype Commands saved.";
                $scope.errorMessage = null;
                $scope.$apply();
            },
            error: function () {
                $scope.errorMessage = "There was an error saving your Hype Commands. Please let Komaru know.";
                $scope.successMessage = null;
                $scope.$apply();
            },
        });
    };
}]);