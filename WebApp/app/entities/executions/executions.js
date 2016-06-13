'use strict';

angular.module('myApp.executions', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/executions', {
    templateUrl: 'entities/executions/executions.html',
    controller: 'ExecutionsCtrl'
  });
}])

.controller('ExecutionsCtrl', ['$scope', '$http', '$rootScope', '$interval',
  function($scope, $http, $rootScope, $interval) {
    $scope.abort = function(id, srv) {
      console.log("abort execution " + id + "" + " on Server " + srv);

      var taskid = JSON.stringify({ TaskID: id });
      var config = JSON.stringify({ServerID : srv, Command : -666, Payload : taskid } );

      console.log(config);

      $http.post("/api/commands", config)
          .success(function(data, status) {
            console.log("Success!!");
          })
          .error(function (data, status, headers, config) {
            console.log('Error');
          });
    };


    $scope.textStatus = function(status) {
      switch (status) {
        case 0:
          return "unknown";
        case 100:
          return "idle";
        case 200:
          return "starting";
        case 300:
          return "running";
        case 1000:
          return "Completed successfully";
        case -2000:
          return "Aborted by user";
        case -3000:
          return "Exception during execution";
        case -3500:
          return "Exception during startup";
        case  -100:
          return "Server presumed dead";
        default:
          return "This should not happen!!!";
      }
    };

    $scope.imgStatus = function(status) {
      var prefix = "imgs/status/";
      switch (status) {
        case 0:
          return prefix + "unknown.png";
        case 100:
          return prefix + "idle.png";
        case 200:
          return prefix + "starting.png";
        case 300:
          return prefix + "running.png";
        case 1000:
          return prefix + "completed.png";
        case -2000:
          return prefix + "aborted.png";
        case -3000:
          return prefix + "exception_execution.png";
        case -3500:
          return prefix + "exception_startup.png";
        case  -100:
          return prefix + "server_dead.png";
        default:
          return prefix + "should_not_happen.png";
      }
    };

    $scope.nowTime = new Date();
    var promise1 = $interval($rootScope.retrieveDeadExecutions, 5000);
    var promise2 = $interval($rootScope.retrieveLiveExecutions, 2000);
    var promise3 = $interval($rootScope.retrieveExecutionQueue, 2000);

    $scope.$on('$destroy', function () {
      $interval.cancel(promise1);
      $interval.cancel(promise2);
      $interval.cancel(promise3);
    });
}]);