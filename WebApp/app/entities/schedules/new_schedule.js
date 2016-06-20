'use strict';

angular.module('myApp.new_schedule', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/new_schedule/:taskID', {
    templateUrl: 'entities/schedules/new_schedule.html',
    controller: 'NewScheduleCtrl'
  });
}])

.controller('NewScheduleCtrl', ['$scope', '$http', '$location', '$routeParams', function($scope, $http, $location, $routeParams) {
  $scope.buttonClass = "btn btn-primary enabled";
  $scope.createInProgess = false;

  $scope.task = null;
  $scope.cron = "";
  $scope.enabled = true;
  
  var uri = "/api/tasks/" +  $routeParams.taskID;

  (function() {
    $http.get(uri)
        .then(function(data) {
          $scope.task = data.data;
          console.log('Task retrieved: ' + $scope.task);
        })
        .catch(function (error) {
          console.log('Error: ' + error.status + ': ' + error.data.ExceptionMessage);
        });
  })();

  $scope.isButtonDisabled = function() {
      return $scope.buttonClass != "btn btn-primary enabled";
  };

  $scope.submitForm = function() {
      $scope.buttonClass = "btn btn-primary disabled";
      $scope.createInProgess = true;

      var schedule = JSON.stringify({
          Cron : $scope.cron,
          Enabled: $scope.enabled,
          TaskID: $routeParams.taskID
      });

      console.log("About to create schedule with " + schedule);

      $http.post("/api/schedules", schedule)
          .then(function(data, status) {
            $location.path("/schedules");
          })
          .catch(function (error) {
            $scope.createInProgess = false;
            $scope.buttonClass = "btn btn-primary enabled";
            $scope.errorMessage = 'Error: ' + error.status + ': ' + error.data.ExceptionMessage;
          });
  };
}]);