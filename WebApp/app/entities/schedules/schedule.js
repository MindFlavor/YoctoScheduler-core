'use strict';

angular.module('myApp.schedule', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/schedule/:schedID', {
    templateUrl: 'entities/schedules/schedule.html',
    controller: 'ScheduleCtrl'
  });
}])

.controller('ScheduleCtrl', ['$scope', '$http', '$routeParams', function($scope, $http, $routeParams) {
  $scope.schedule = null;

  $scope.retrieveSchedule = function () {
    var id = $routeParams.schedID;
    console.log('Retrieving schedule ' + id)
    $http.get('api/schedules/' + id).
    then(function (data) {
      $scope.schedule = data.data;
    }).
    catch(function (data) {
      console.log('schedule ' + id  + ' *not* retrieved')
      $scope.schedule = null;
    });
  };

  $scope.retrieveSchedule();
}]);