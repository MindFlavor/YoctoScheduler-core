'use strict';

angular.module('myApp.new_task_powershell', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/tasks/new_task_powershell', {
    templateUrl: 'entities/task/new/new_task_powershell.html',
    controller: 'NewTaskPowerShellCtrl'
  });
}])

.controller('NewTaskPowerShellCtrl', ['$rootScope', '$scope', '$http', '$location', 'newTaskDetails', function($rootScope, $scope, $http, $location, newTaskDetails) {
  $scope.newTaskDetails = newTaskDetails;

  $scope.taskScript = "";

  $scope.buttonClass = "btn btn-primary enabled";
  $scope.createInProgess = false;
  $scope.errorMessage = "";

  $scope.createTask = function() {
    $scope.buttonClass = "btn btn-primary disabled";
    $scope.createInProgess = false;
    
    var payload = JSON.stringify({Script: $scope.taskScript });
    var task = JSON.stringify({
      Name : $scope.newTaskDetails.name,
      Description: $scope.newTaskDetails.description,
      Type: "PowerShellTask",
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