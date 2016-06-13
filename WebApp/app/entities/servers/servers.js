'use strict';

angular.module('myApp.servers', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/servers', {
    templateUrl: 'entities/servers/servers.html',
    controller: 'ServersCtrl'
  });
}])

.controller('ServersCtrl', ['$scope', '$http', '$rootScope', '$interval', function($scope, $http, $rootScope, $interval) {
  var promise = $interval($rootScope.retrieveServers, 2000);

  $scope.$on('$destroy', function () {
    $interval.cancel(promise);
  });
}]);