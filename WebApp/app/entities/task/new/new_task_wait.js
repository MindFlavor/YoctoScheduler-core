'use strict';

angular.module('myApp.new_task_wait', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/tasks/new_task_wait', {
    templateUrl: 'entities/task/new/new_task_wait.html',
    controller: 'NewTaskWaitCtrl'
  });
}])

.controller('NewTaskWaitCtrl', ['$rootScope', '$scope', '$http', '$location', 'newTaskDetails', function($rootScope, $scope, $http, $location, newTaskDetails) {
  $scope.newTaskDetails = newTaskDetails;

  $scope.waitSeconds = 60;

  $scope.buttonClass = "btn btn-primary enabled";
  $scope.createInProgess = false;
  $scope.errorMessage = "";

  
  $scope.createTask = function() {
    $scope.buttonClass = "btn btn-primary disabled";
    $scope.createInProgess = false;
    
    var payload = JSON.stringify({SleepSeconds: $scope.waitSeconds});
    var task = JSON.stringify({
      Name : $scope.newTaskDetails.name,
      Description: $scope.newTaskDetails.description,
      Type: "WaitTask",
      ReenqueueOnDead : $scope.newTaskDetails.cbRequeueOnDead,
      ConcurrencyLimitGlobal: $scope.newTaskDetails.globalLimit,
      ConcurrencyLimitSameInstance : $scope.newTaskDetails.localLimit,
      Payload : payload});

    console.log("About to create task with " + task);

    $http.post("/api/tasks", task)
        .success(function(data, status) {
          $rootScope.initializeNewTaskDetails($scope.newTaskDetails);
          $location.path("/task/" + data.ID);
        })
        .error(function (data, status, headers, config) {
          $scope.createInProgess = false;
          $scope.errorMessage = 'Error: ' + status + ', ' + data + '.';
        });
  }
}]);