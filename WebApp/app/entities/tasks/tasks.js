'use strict';

angular.module('myApp.tasks', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/tasks', {
    templateUrl: 'entities/tasks/tasks.html',
    controller: 'TasksCtrl'
  });
}])

.controller('TasksCtrl', ['$scope', '$http', '$rootScope', function($scope, $http, $rootScope) {
  $rootScope.retrieveTasks();

  $scope.getButtonImage = function(taskID, priority) {
    if ($scope.isButtonDisabled(taskID))
      return "imgs/enqueue_disabled.png";
    else if(priority === "high")
      return "imgs/enqueue_high_priority.png";
    else
      return "imgs/enqueue.png";
  };

  $scope.getButtonClass = function(taskID) {
    if ($scope.isButtonDisabled(taskID))
      return "btn btn-default disabled";
    else
      return "btn btn-default enabled";
  };

  $scope.getButtonTitle = function(taskID) {
    if ($scope.isButtonDisabled(taskID))
      return "Processing request...";
    else
      return "Add this task directly to the execution queue";
  };
  
  $scope.isButtonDisabled = function(taskID) {
    return !$rootScope.findTaskByID(taskID).isEnabled;
  };

  $scope.enqueueTask = function(taskID, priority) {
    var item = $rootScope.findTaskByID(taskID);
    item.isEnabled = false;

    var priorityInt = 0;
    if(priority == 'high')
        priorityInt = 500;

    var reqJSON = JSON.stringify({TaskID : taskID, Priority : priorityInt});

    console.log('ecco: ' + reqJSON);

    $http.post('api/queueitems', reqJSON).
    success(function (data) {
      item.isEnabled = true;
    }).
    error(function (data, status, headers, config) {
      console.log('Enqueueing failed');
      item.isEnabled = true;
    });
  };
}]);