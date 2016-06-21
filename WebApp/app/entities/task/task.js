'use strict';

angular.module('myApp.task', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/task/:taskID', {
    templateUrl: 'entities/task/task.html',
    controller: 'TaskCtrl'
  });
}])

.controller('TaskCtrl', ['$scope', '$http', '$location', '$routeParams', function($scope, $http, $location, $routeParams) {
  $scope.task = null;

  $scope.retrieveTask = function (serverID) {
    var id =
    console.log('Retrieving task ' + serverID);
    $http.get('api/tasks/' + serverID).
    then(function (data) {
      console.log('Task ' + serverID  + ' retrieved');
      $scope.task = data.data
    }).
    catch(function (data, status, headers, config) {
      console.log('Task ' + serverID  + ' *not* retrieved');
      $scope.task = null
    });
  };

  $scope.retrieveTask($routeParams.taskID);
}]);