'use strict';

angular.module('myApp.task', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/task', {
    templateUrl: 'entities/task/task.html',
    controller: 'TaskCtrl'
  });
}])

.controller('TaskCtrl', ['$scope', '$http', '$location', function($scope, $http, $location) {
  if ($scope.task === undefined) {
    $scope.task = null;
  }

  $scope.retrieveTask = function () {
    var id = $location.search()['id']
    console.log('Retrieving task ' + id)
    $http.get('api/tasks/' + id).
    success(function (data) {
      console.log('Task ' + id  + ' retrieved')
      $scope.task = data
    }).
    error(function (data, status, headers, config) {
      console.log('Task ' + id  + ' *not* retrieved')
      $scope.task = null
    });
  };

  if ($scope.task === null) {
    $scope.retrieveTask();
  }
}]);