'use strict';

angular.module('myApp.new_task_ssis', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/tasks/new_task_ssis', {
    templateUrl: 'entities/task/new/new_task_ssis.html',
    controller: 'NewTaskSSISCtrl'
  });
}])

.controller('NewTaskSSISCtrl', ['$rootScope', '$scope', '$http', '$location', 'newTaskDetails', function($rootScope, $scope, $http, $location, newTaskDetails) {
  $scope.newTaskDetails = newTaskDetails;

  $scope.bitModel = "64bit";
  $scope.sqlVersion = "120";
  $scope.timeout = 0;
  $scope.taskArguments = "";

  $scope.buttonClass = "btn btn-primary enabled";
  $scope.createInProgess = false;
  $scope.errorMessage = "";

  $scope.createTask = function() {
    $scope.buttonClass = "btn btn-primary disabled";
    $scope.createInProgess = false;

    var use32bit = true;
    if($scope.bitModel === "64bit")
        use32bit = false;

    var sqlVersion = 120;
    switch($scope.sqlVersion)
    {
      case "100":
        sqlVersion = 100;
        break;
      case "110":
        sqlVersion = 110;
        break;
      case "120":
        sqlVersion = 120;
        break;
      case "130":
        sqlVersion = 130;
        break;
      case "140":
        sqlVersion = 140;
        break;
    }

    var payload = JSON.stringify({Use32Bit: use32bit, SQLVersion: sqlVersion, Arguments: $scope.taskArguments, Timeout: $scope.timeout});
    var task = JSON.stringify({
      Name : $scope.newTaskDetails.name,
      Description: $scope.newTaskDetails.description,
      Type: "SSISTask",
      ReenqueueOnDead : $scope.newTaskDetails.cbRequeueOnDead,
      ConcurrencyLimitGlobal: $scope.newTaskDetails.globalLimit,
      ConcurrencyLimitSameInstance : $scope.newTaskDetails.localLimit,
      Payload : payload});

    console.log("About to create task with " + task);

    $http.post("/api/tasks", task)
        .success(function(data, status) {
          $rootScope.initializeNewTaskDetails($scope.newTaskDetails);
          $location.path("/task").search({id: data.ID});
        })
        .error(function (data, status, headers, config) {
          $scope.createInProgess = false;
          $scope.errorMessage = 'Error: ' + status + ', ' + data + '.';
        });
  }
}]);