'use strict';

angular.module('myApp.new_secret', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/new_secret', {
    templateUrl: 'entities/secrets/new_secret.html',
    controller: 'NewSecretCtrl'
  });
}])

.controller('NewSecretCtrl', ['$scope', '$http', '$location', function($scope, $http, $location) {
  $scope.name = "";
  $scope.thumbprint = "";
  $scope.plainTextValue = "";

  $scope.createInProgess = false;
  $scope.buttonClass = "btn btn-primary disabled";
  $scope.buttonTitle = "Button is disabled because name is empty";
  $scope.errorMessage = "";

  $scope.validateInput = function() {
    // disable until completed check
    $scope.buttonClass = "btn btn-primary disabled";

    if(($scope.name == "") || ($scope.name === undefined) ) {
      $scope.buttonClass = "btn btn-primary disabled";
      $scope.buttonTitle = "Button is disabled because name is empty";
    }
    else {
      // check for existence
      $scope.checkInProgess = true;
      var uri = "/api/encryptedsecretitems/" + encodeURIComponent($scope.name);

      $http.get(uri)
          .success(function(data, status) {
            $scope.buttonClass = "btn btn-primary disabled";
            $scope.buttonTitle = "Button is disabled because name is already in use";
            $scope.checkInProgess = false;
          })
          .error(function (data, status, headers, config) {
            if(($scope.name == "") || ($scope.name === undefined) ) {
              $scope.buttonClass = "btn btn-primary disabled";
              $scope.buttonTitle = "Button is disabled because name is empty";
            } else {
              $scope.buttonClass = "btn btn-primary enabled";
              $scope.buttonTitle = "";
              $scope.checkInProgess = false;
            }
          });
    }
  };
  
  $scope.isButtonEnabled = function() {
    return $scope.buttonClass != "btn btn-primary enabled";
  };

  $scope.submitForm = function() {
    $scope.buttonClass = "btn btn-primary disabled";
    $scope.createInProgess = true;

    var secret = JSON.stringify({
      Name : $scope.name,
      CertificateThumbprint: $scope.thumbprint,
      PlainTextValue: $scope.plainTextValue
    });

    console.log("About to create secret with " + secret);

    $http.post("/api/plaintextsecretitems", secret)
        .then(function(data, status) {
          $location.path("/secrets");
        })
        .catch(function (error) {
          $scope.createInProgess = false;
          $scope.errorMessage = 'Error: ' + error.status + ': ' + error.data.ExceptionMessage;
        });
  };
}]);