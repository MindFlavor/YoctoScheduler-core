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
  }

  $scope.retrieveSchedules = function () {
    console.log('Retrieving schedules')
    $http.get('api/schedules').
    success(function (data) {
      console.log('Data retrieved from site')
      $scope.schedules = data
    }).
    error(function (data, status, headers, config) {
      console.log('Data *not* retrieved')
      $scope.schedules = null
    });
  };

  if ($scope.schedules === null) {
    $scope.retrieveSchedules();
  }
}]);