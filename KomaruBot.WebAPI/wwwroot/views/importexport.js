'use strict';

angular.module('KomaruBot')
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/importexport', {
        templateUrl: 'views/importexport.html',
        controller: 'ImportExportController'
    });
}])
.controller('ImportExportController', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
    $scope.isLoggedIn = authService.isLoggedIn();

    $scope.exportLink = "api/settings/botsettings/all?auth=" + window.sessionStorage["accesstoken"];

    $scope.loaded = true;

    $scope.ImportLog = "";

    $scope.importFile = function () {
        var fr = new FileReader();

        $scope.ImportLog = "";

        fr.onload = function (e) {
            var result = null;
            try {
                result = JSON.parse(e.target.result);
            }
            catch (ex) {
                $scope.ImportLog += "There was an error opening the selected file. Did you load the correct one?" + "\r\n";
                $scope.$apply();
                return;
            }

            if (result == null) {
                $scope.ImportLog += "There was an error parsing the selected file. Did you load the correct one?" + "\r\n";
                $scope.$apply();
                return;
            }

            if (result.botSettings == null &&
                result.ceresSettings == null &&
                result.gambleSettings == null &&
                result.hypeSettings == null) {

                $scope.ImportLog += "There was an error parsing the selected file's data. Did you load the correct one?" + "\r\n";
                $scope.$apply();
                return;
            }

            var updateSettings = function (settings, settingsName, endpoint) {
                $scope.ImportLog += "Importing " + settingsName + " settings..." + "\r\n";
                $scope.$apply();

                $.PerformHttpRequest({
                    type: "PUT",
                    url: endpoint,
                    queryString: null,
                    data: settings,
                    loadingIcon: null,
                    error: null,
                    success: function (json) {
                        $scope.ImportLog += "Imported " + settingsName + " settings." + "\r\n";
                        $scope.$apply();
                    },
                    error: function (response) {
                        if (response != null && response.responseJSON != null && response.responseJSON.message != null) {
                            $scope.ImportLog += "There was an error importing your " + settingsName + " settings: " + response.responseJSON.message + "\r\n";
                        } else {
                            $scope.ImportLog += "There was an error importing your " + settingsName + " settings. Please let Komaru know." + "\r\n";
                        }
                        $scope.$apply();
                    },
                });
            };

            if (result.botSettings != null) {
                updateSettings(result.botSettings, "Account", "api/settings/botsettings");
            }

            if (result.ceresSettings != null) {
                updateSettings(result.ceresSettings, "Ceres", "api/settings/ceres");
            }

            if (result.gambleSettings != null) {
                updateSettings(result.gambleSettings, "Gamble", "api/settings/gamble");
            }

            if (result.hypeSettings != null) {
                updateSettings(result.hypeSettings, "Hype Commands", "api/settings/hype");
            }
        };

        var files = document.getElementById('selectFiles').files;
        fr.readAsText(files.item(0));
    }

}]);