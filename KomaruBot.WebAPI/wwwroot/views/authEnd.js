'use strict';

angular.module('KomaruBot')
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/access_:details', { // Tricky route, Twitch actually sends "#access_token=blah&whatever=something else
        templateUrl: 'views/authEnd.html',
        controller: 'AuthEndController'
    });
}])
.controller('AuthEndController', ['$scope', '$route', '$routeParams', '$location', '$rootScope', function ($scope, $route, $routeParams, $location, $rootScope) {

    var params = $routeParams.details;
    var paramsSplit = params.split("&");
    var i = 0;
    for (i = 0; i < paramsSplit.length; i++) {
        var individualParam = paramsSplit[i];
        var individualParamsplit = individualParam.split("=");
        var key = individualParamsplit[0];
        var value = individualParamsplit[1];

        if (key == "token") {
            // Save the access token for this session:
            window.sessionStorage["accesstoken"] = value;

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
                        setTimeout(function () { location.reload(); }, 0); 
                    } else {
                        alert("There was an error logging in. Result was " + json.resultString + ". Message: " + json.message);
                    }
                },
            });

            return;
        }
    }
        
}]);