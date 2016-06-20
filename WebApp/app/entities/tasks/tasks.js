'use strict';

angular.module('myApp.tasks', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/tasks', {
    templateUrl: 'entities/tasks/tasks.html',
    controller: 'TasksCtrl'
  });
}])

.controller('TasksCtrl', ['$scope', '$http', function($scope, $http) {
  $scope.tasks = null;

  $scope.findTaskByID = function(taskID) {
    for (var i = 0; i < $scope.tasks.length; i++) {
      if ($scope.tasks[i].ID === taskID)
        return $scope.tasks[i];
    }
  };

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
    return !$scope.findTaskByID(taskID).isEnabled;
  };

  $scope.enqueueTask = function(taskID, priority) {
    var item = $scope.findTaskByID(taskID);
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
  
  $scope.retrieveTasks = function () {
    console.log('Retrieving tasks')
    $http.get('api/tasks').
    success(function (data) {
      console.log('Data retrieved from site')
      angular.forEach(data, function(elem) {
        elem.isEnabled = true;
      });
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