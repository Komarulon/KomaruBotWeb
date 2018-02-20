'use strict';

angular.module('KomaruBot')
.service('authService', [function () {
    this.isLoggedIn = function () {
        var token = window.sessionStorage["accesstoken"];
        if (token == null) {
            return false;
        }
        return true;
    }
}]);