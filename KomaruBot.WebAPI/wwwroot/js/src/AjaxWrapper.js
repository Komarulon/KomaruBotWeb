// Anytime an AJAX method is written, it should come through this function instead


var PerformHttpRequest = function (jquerySettings) {
    /* callers can specify:
    responseDataType - Response Data Type expected. Defaults to json
    loadingIcon - auto show/hide during the request
    error - on error function
    success - on success function
    url - url to load (don't add query string values)
    queryString - object containing values for the querystring. Dates/DateTimes should remain in original format
    data - object containing data to be sent with the request
    type: HTTP Method ("GET", "POST", etc)
    */

    // Callers can send the value "loadingIcon" to have a loading icon/gif/whatever automatically show/hide
    var loadingIcon = jquerySettings.loadingIcon;
    if (loadingIcon != null && typeof loadingIcon === "string") {
        loadingIcon = $(loadingIcon);
    }

    var loadingIconStop = function () {
        if (loadingIcon != null) {
            $(loadingIcon).hide();
        }
    };

    // By default, hide the loading icon and alert that there was an error
    var failFunc = function (xmlHttpRequest) {
        loadingIconStop();
        if (xmlHttpRequest.readyState == 0 || xmlHttpRequest.status == 0)
            return;  // it's not really an error
        alert("There was an error processing your request. Please contact Komaru for help with this error.");
    };

    // if the caller specified something else, hide the loading icon, and call their error function
    if (jquerySettings.error != null) {
        failFunc = function (xmlHttpRequest) {
            loadingIconStop();
            if (xmlHttpRequest.readyState == 0 || xmlHttpRequest.status == 0)
                return;  // it's not really an error
            jquerySettings.error(xmlHttpRequest);
        };
    }

    try {

        // On success, hide the loading icon by default
        var successFunc = function (responseData) {
            loadingIconStop();
        };

        // If the caller specified a success function, hide the loading icon, then call the success function
        if (jquerySettings.success != null) {
            successFunc = function (responseData) {
                loadingIconStop();
                jquerySettings.success(responseData);
            };
        }

        var url = jquerySettings.url;

        // Callers can pass in a string queryString like "?a=123&b=123" or an object like { a: 123, b: 123 }
        if (jquerySettings.queryString != null) {
            if (typeof jquerySettings.queryString === "string") {
                url += jquerySettings.queryString;
            } else {
                var isFirst = true;
                var compiledQString = "";
                for (var key in jquerySettings.queryString) {

                    // normally you could just use $.param() which works fine, but let's make one that handles datetimes ok:
                    compiledQString += (isFirst ? "?" : "&");

                    var val = "";
                    var objVal = jquerySettings.queryString[key];
                    if (objVal == null) {
                        val = "null";
                    } else if (typeof objVal === "object" && objVal.toISOString != null) { // If it's a datetime, use the ISO 8601 time:
                        val = objVal.toISOString()
                    } else {
                        val = objVal.toString();
                    }

                    compiledQString += encodeURIComponent(key) + "=" + encodeURIComponent(val);

                    isFirst = false;
                }

                url += compiledQString;
            }
        }

        var httpMethod = "GET";
        if (jquerySettings.type != null) {
            httpMethod = jquerySettings.type;
        }

        var postData = null;
        var contentType = null;
        if (jquerySettings.data != null) {
            postData = JSON.stringify(jquerySettings.data);
            contentType = 'application/json'; // Specify contentType when sending data
        }

        var responseDataType = "json";
        if (jquerySettings.responseDataType != null) {
            responseDataType = jquerySettings.responseDataType;
        }

        var authToken = window.sessionStorage["accesstoken"];

        var settings = {
            url: url,
            type: httpMethod,

            contentType: contentType,
            data: postData,

            dataType: responseDataType,
            success: successFunc,
            error: failFunc,
            beforeSend: function (xhr) {
                if (authToken != null) {
                    xhr.setRequestHeader('Authorization', "Bearer " + authToken);
                }
            },
        };

        if (loadingIcon != null) {
            $(loadingIcon).css("display", "inline-block");
        }

        $.ajax(settings)
    } catch (exc) {
        console.error("An exception occurred before sending the Ajax request. Error:");
        console.error(exc);
        failFunc(null);
    }
};

(function ($) {
    $.extend({
        PerformHttpRequest: PerformHttpRequest
    });
})(jQuery);

