'use strict';

angular.module('myApp.server', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/server/:serverID', {
    templateUrl: 'entities/servers/server.html',
    controller: 'ServerCtrl'
  });
}])

.controller('ServerCtrl', ['$scope', '$http', '$routeParams', function($scope, $http, $routeParams) {
  $scope.server = null;

  $scope.retrieveServer = function (serverID) {
    console.log('Retrieving server ' + serverID)
    $http.get('api/servers/' + serverID).
    then(function (data) {
      $scope.server = data.data;
    }).
    catch(function (data) {
      console.log('server ' + serverID  + ' *not* retrieved')
      $scope.server = null;
    });
  };

  $scope.retrieveServer($routeParams.serverID);
}]);