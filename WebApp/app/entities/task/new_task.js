'use strict';

angular.module('myApp.new_task', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/new_task', {
    templateUrl: 'entities/task/new_task.html',
    controller: 'NewTaskCtrl'
  });
}])

.controller('NewTaskCtrl', ['$scope', '$http', '$location', 'newTaskDetails', function($scope, $http, $location, newTaskDetails) {
  $scope.newTaskDetails = newTaskDetails;

  $scope.cbGlobalLimit = false;
  $scope.cbLocalLimit = false;

  $scope.checkInProgess = false;
  if(($scope.newTaskDetails.name === "")|| (($scope.newTaskDetails.name === undefined)))
    $scope.buttonClass = "btn btn-primary disabled";
  else
    $scope.buttonClass = "btn btn-primary enabled";

  $scope.buttonTitle = "Button is disabled because name is empty";

  $scope.validateInput = function() {
    // disable until completed check
    $scope.buttonClass = "btn btn-primary disabled";

    if(($scope.newTaskDetails.name == "") || ($scope.newTaskDetails.name === undefined) ) {
      $scope.buttonClass = "btn btn-primary disabled";
      $scope.buttonTitle = "Button is disabled because name is empty";
    }
    else {
      // check for existence
      $scope.checkInProgess = true;
      var uri = "/api/tasks/" + encodeURIComponent($scope.newTaskDetails.name) + "?type=string";

      $http.get(uri)
          .success(function(data, status) {
              $scope.buttonClass = "btn btn-primary disabled";
              $scope.buttonTitle = "Button is disabled because name is already in use";
              $scope.checkInProgess = false;
          })
          .error(function (data, status, headers, config) {
            if(($scope.newTaskDetails.name == "") || ($scope.newTaskDetails.name === undefined) ) {
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

  $scope.redirectToTask = function(){
    if(!$scope.cbGlobalLimit)
      newTaskDetails.globalLimit = 0;
    if(!$scope.cbLocalLimit)
      newTaskDetails.localLimit = 0;

    switch($scope.newTaskDetails.selectedTask) {
      case "T-SQL":
        $location.path("/tasks/new_task_tsql");
        break;
      case "SSIS":
        $location.path("/tasks/new_task_ssis");
        break;
      case "PowerShell":
        $location.path("/tasks/new_task_powershell");
        break;
      case "Wait":
        $location.path("/tasks/new_task_wait");
        break;
    }
  };
}]);