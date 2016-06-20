'use strict';

angular.module('myApp.schedules', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/schedules', {
    templateUrl: 'entities/schedules/schedules.html',
    controller: 'SchedulesCtrl'
  });
}])

.controller('SchedulesCtrl', ['$scope', '$http', function($scope, $http) {
  if ($scope.schedules === undefined) {
    $scope.schedules = null;
  };

  $scope.findScheduleByID = function(schedID) {
    for (var i = 0; i < $scope.schedules.length; i++) {
      if ($scope.schedules[i].ID === schedID)
        return $scope.schedules[i];
    }
  };

  $scope.getButtoncClass = function(schedID) {
      return $scope.findScheduleByID(schedID).style;
    };

  $scope.isButtonDisabled = function(schedID) {
    return $scope.findScheduleByID(schedID).style === "btn btn-danger disabled";
  };

  $scope.retrieveSchedules = function () {
    console.log('Retrieving schedules')
    $http.get('api/schedules').
    success(function (data) {
      console.log('Data retrieved from site');

      angular.forEach(data, function(elem) {
        elem.style = "btn btn-danger enabled";
      });

      $scope.schedules = data;
    }).
    error(function (data, status, headers, config) {
      console.log('Data *not* retrieved')
      $scope.schedules = null
    });
  };

  $scope.removeSchedule = function(scheduleID) {
    console.log('Removing scheduleID == ' + scheduleID);

    var item = $scope.findScheduleByID(scheduleID);
    var itemIndex = $scope.schedules.indexOf(item);
    item.style = "btn btn-danger disabled";

    var uri = "api/schedules/" + scheduleID;

    $http.delete(uri).
        then(function(data) {
      $scope.schedules.splice(itemIndex);
    }).catch(function(error) {
      item.style = "btn btn-danger enabled";
    });
  };

  if ($scope.schedules === null) {
    $scope.retrieveSchedules();
  }
}]);