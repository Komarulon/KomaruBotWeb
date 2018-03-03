'use strict';

angular.module('KomaruBot')
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/main', {
        templateUrl: 'views/main.html',
        controller: 'MainController'
    });
}])
.controller('MainController', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
    $scope.isLoggedIn = authService.isLoggedIn();
    if (!$scope.isLoggedIn || (window.sessionStorage["username"] == null)) { $location.path('/preauth'); setTimeout(function () { location.reload(); }, 0); }

    $scope.userID = window.sessionStorage["username"];
    $scope.streamElementsJWTToken = null;
    $scope.streamElementsAccountID = null;
    $scope.currencySingular = null;
    $scope.currencyPlural = null;
    $scope.botEnabled = false;

    $scope.loaded = false;
    $scope.loadingMessage = "Loading...";
    $scope.errorMessage = null;
    $scope.successMessage = null;
    

    $.PerformHttpRequest({
        type: "GET",
        url: "api/settings/botsettings",
        queryString: null,
        data: null,
        loadingIcon: null,
        error: null,
        success: function (json) {
            $scope.userID = json.userID;
            $scope.streamElementsJWTToken = json.streamElementsJWTToken;
            $scope.streamElementsAccountID = json.streamElementsAccountID;
            $scope.currencySingular = json.currencySingular;
            $scope.currencyPlural = json.currencyPlural;
            $scope.botEnabled = json.botEnabled;

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
                $scope.errorMessage = "There was an error loading your Account settings. Please let Komaru know.";
                $scope.successMessage = null;
            }
            $scope.$apply();
        },
    });

    $scope.saveSettings = function () {
        var model = {
            streamElementsJWTToken: $scope.streamElementsJWTToken,
            streamElementsAccountID: $scope.streamElementsAccountID,
            currencySingular: $scope.currencySingular,
            currencyPlural: $scope.currencyPlural,
            userID: $scope.userID,
            botEnabled: $scope.botEnabled,
        };

        $scope.loadingMessage = "Saving Account settings...";
        $scope.errorMessage = null;
        $scope.successMessage = null;

        $.PerformHttpRequest({
            type: "PUT",
            url: "api/settings/botsettings",
            queryString: null,
            data: model,
            loadingIcon: null,
            error: null,
            success: function (json) {

                $scope.loadingMessage = null;
                $scope.errorMessage = null;
                $scope.successMessage = "Account settings saved.";
                $scope.$apply();

                setTimeout(function () {
                    if ($scope.successMessage == "Account settings saved.") {
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
                    $scope.errorMessage = "There was an error saving your Account settings. Please let Komaru know.";
                    $scope.successMessage = null;
                }
                $scope.$apply();
            },
        });
    };

}]);