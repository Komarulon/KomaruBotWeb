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
    $scope.botAccount = null;
    $scope.basicBotConfiguration = null;

    $scope.loaded = false;
    $scope.loadingMessage = "Loading...";
    $scope.errorMessage = null;
    $scope.successMessage = null;

    $scope.originallyBotEnabled = false;

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
            $scope.basicBotConfiguration = json.basicBotConfiguration;
            if (json.linkedBotAccount != null) {
                $scope.botAccount = json.linkedBotAccount;
            } else {
                $scope.botAccount = "KomaruBot";
            }

            $scope.originallyBotEnabled = json.botEnabled;

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
            basicBotConfiguration: $scope.basicBotConfiguration,
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

                if ($scope.originallyBotEnabled == false && model.botEnabled)
                {
                    $scope.successMessage = "Account settings saved. Bot is now enabled. Make sure to check other tabs to enable features you want on.";
                }
                else
                {
                    $scope.successMessage = "Account settings saved.";    
                }

                $scope.originallyBotEnabled = model.botEnabled;

                $scope.$apply();

                setTimeout(function () {
                    if ($scope.successMessage == "Account settings saved." ||
                        $scope.successMessage == "Account settings saved. Bot is now enabled. Make sure to check other tabs to enable features you want on.") {
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


    $scope.removeBotLink = function () {
        if (!confirm("Are you sure you want to unlink " + $scope.botAccount + "? KomaruBot will communicate through the Twitch Account KomaruBot instead.")) {
            return;
        }

        $.PerformHttpRequest({
            type: "DELETE",
            url: "api/settings/botsettings/botuser",
            queryString: null,
            data: null,
            loadingIcon: null,
            error: null,
            success: function (json) {
                $scope.loadingMessage = null;
                $scope.errorMessage = null;
                $scope.successMessage = "Bot Account Unlinked.";

                $scope.botAccount = "KomaruBot";
                $scope.$apply();

                setTimeout(function () {
                    if ($scope.successMessage == "Bot Account unlinked.") {
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
                    $scope.errorMessage = "There was an error unlinking the Bot Account settings. Please let Komaru know.";
                    $scope.successMessage = null;
                }
                $scope.$apply();
            },
        });
    };

    $scope.beginBotLink = function () {

        if ($scope.newBotAccount == null || $scope.newBotAccount == "") {
            alert("Please enter an account username.");
            return;
        }

        var model = {
            requestedTwitchUsername: $scope.newBotAccount,
        };
        $.PerformHttpRequest({
            type: "POST",
            url: "api/settings/botsettings/botuser",
            queryString: null,
            data: model,
            loadingIcon: null,
            error: null,
            success: function (json) {
                $scope.loadingMessage = null;
                $scope.errorMessage = null;
                $scope.successMessage = "Bot Link Request sent to " + model.requestedTwitchUsername + ". Log in as " + model.requestedTwitchUsername + " to complete the link.";

                $scope.$apply();
            },
            error: function (response) {
                if (response != null && response.responseJSON != null && response.responseJSON.message != null) {
                    $scope.loadingMessage = null;
                    $scope.errorMessage = response.responseJSON.message;
                    $scope.successMessage = null;
                } else {
                    $scope.loadingMessage = null;
                    $scope.errorMessage = "There was an error sending the Bot Link Request. Please let Komaru know.";
                    $scope.successMessage = null;
                }
                $scope.$apply();
            },
        });
    };

    // See if there's any pending requests
    $.PerformHttpRequest({
        type: "GET",
        url: "api/settings/botsettings/botuser",
        queryString: null,
        data: null,
        loadingIcon: null,
        error: null,
        success: function (json) {

            $scope.botAccountRequestUsername = json.requestingUsername;

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
                $scope.errorMessage = "There was an error loading your Bot Linking settings. Please let Komaru know.";
                $scope.successMessage = null;
            }
            $scope.$apply();
        },
    });

    $scope.acceptBotLink = function () {
        if (!confirm("Are you sure you want to allow your account (" + $scope.userID + ") to talk in chat for Twitch user " + $scope.botAccountRequestUsername + "?")) {
            return;
        }

        var model = {
            requestingTwitchUsername: $scope.botAccountRequestUsername,
        };
        
        $.PerformHttpRequest({
            type: "PUT",
            url: "api/settings/botsettings/botuser",
            queryString: null,
            data: model,
            loadingIcon: null,
            error: null,
            success: function (json) {
                $scope.loadingMessage = null;
                $scope.errorMessage = null;
                $scope.successMessage = "Your account has been linked as a bot for " + $scope.botAccountRequestUsername;

                $scope.$apply();
            },
            error: function (response) {
                if (response != null && response.responseJSON != null && response.responseJSON.message != null) {
                    $scope.loadingMessage = null;
                    $scope.errorMessage = response.responseJSON.message;
                    $scope.successMessage = null;
                } else {
                    $scope.loadingMessage = null;
                    $scope.errorMessage = "There was an error accepting the Bot Link Request. Please let Komaru know.";
                    $scope.successMessage = null;
                }
                $scope.$apply();
            },
        });
    };
}]);