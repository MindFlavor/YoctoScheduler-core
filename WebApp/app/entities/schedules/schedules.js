'use strict';

angular.module('myApp.schedules', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/schedules', {
    templateUrl: 'entities/schedules/schedules.html',
    controller: 'SchedulesCtrl'
  });
}])

.controller('SchedulesCtrl', ['$rootScope', '$scope', '$http', function($rootScope, $scope, $http) {
  $rootScope.retrieveSchedules();

  $scope.getButtoncClass = function(schedID) {
      return $rootScope.findScheduleByID(schedID).style;
    };

  $scope.isButtonDisabled = function(schedID) {
    return $rootScope.findScheduleByID(schedID).style === "btn btn-danger disabled";
  };

  $scope.removeSchedule = function(scheduleID) {
    console.log('Removing scheduleID == ' + scheduleID);

    var item = $rootScope.findScheduleByID(scheduleID);
    var itemIndex = $rootScope.schedules.indexOf(item);
    item.style = "btn btn-danger disabled";

    var uri = "api/schedules/" + scheduleID;

    $http.delete(uri).
        then(function(data) {
      $rootScope.schedules.splice(itemIndex);
    }).catch(function(error) {
      item.style = "btn btn-danger enabled";
    });
  };
}]);