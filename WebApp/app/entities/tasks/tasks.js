'use strict';

angular.module('myApp.tasks', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/tasks', {
    templateUrl: 'entities/tasks/tasks.html',
    controller: 'TasksCtrl'
  });
}])

.controller('TasksCtrl', ['$scope', '$http', function($scope, $http) {
  if ($scope.tasks === undefined) {
    $scope.tasks = null;
  }
  
  $scope.retrieveTasks = function () {
    console.log('Retrieving tasks')
    $http.get('api/tasks').
    success(function (data) {
      console.log('Data retrieved from site')
      $scope.tasks = data
    }).
    error(function (data, status, headers, config) {
      console.log('Data *not* retrieved')
      $scope.tasks = null
    });
  };

  if ($scope.tasks === null) {
    $scope.retrieveTasks();
  }
}]);