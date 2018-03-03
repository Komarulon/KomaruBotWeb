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
            $scope.ceresEnabled = json.ceresConfiguration.ceresEnabled;

            $scope.loaded = true;
            $scope.errorMessage = null;
            $scope.$apply();
        },
        error: function () {
            $scope.errorMessage = "There was an error loading your Ceres settings. Please let Komaru know.";
            $scope.successMessage = null;
            $scope.$apply();
        },
    });

    $scope.saveSettings = function () {
        var model = {
            userID: $scope.userID,
            ceresConfiguration: {
                ceresEnabled: $scope.ceresEnabled,
            }
        };

        $.PerformHttpRequest({
            type: "PUT",
            url: "api/settings/ceres",
            queryString: null,
            data: model,
            loadingIcon: null,
            error: null,
            success: function (json) {
                $scope.successMessage = "Ceres settings saved.";
                $scope.errorMessage = null;
                $scope.$apply();
            },
            error: function () {
                $scope.errorMessage = "There was an error saving your Ceres settings. Please let Komaru know.";
                $scope.successMessage = null;
                $scope.$apply();
            },
        });
    };
}]);