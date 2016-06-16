'use strict';

angular.module('myApp.secrets', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/secrets', {
    templateUrl: 'entities/secrets/secrets.html',
    controller: 'SecretsCtrl'
  });
}])

.controller('SecretsCtrl', ['$scope', '$http', function($scope, $http) {
  $scope.cbShowValue = false

  if ($scope.secrets === undefined) {
    $scope.secrets = null;
  }

  $scope.retrieveSecrets = function () {
    console.log('Retrieving secrets')
    $http.get('api/encryptedsecretitems').
    success(function (data) {
      console.log('encryptedsecretitems Data retrieved from site')
      $scope.secrets = data
    }).
    error(function (data, status, headers, config) {
      console.log('encryptedsecretitems Data *not* retrieved')
      $scope.secrets = null
    });
  };

  if ($scope.secrets === null) {
    $scope.retrieveSecrets();
  }
}]);