'use strict';

// Declare app level module which depends on views, and components
angular.module('myApp', [
  'ngRoute',
  'angularMoment',
  'myApp.servers', 
  'myApp.tasks',
  'myApp.schedules',
  'myApp.view2',
  'myApp.version',
  'myApp.executions',
  'myApp.task',
  'myApp.secrets',
  'myApp.new_task',
  'myApp.new_task_wait',
  'myApp.new_task_ssis',
  'myApp.new_task_tsql',
  'myApp.new_task_powershell',
  'myApp.new_secret',
  'myApp.new_schedule',
  'myApp.schedule',
  'myApp.server'
]).
config(['$locationProvider', '$routeProvider', function($locationProvider, $routeProvider) {
  $locationProvider.hashPrefix('!');

  //$routeProvider.otherwise({redirectTo: '/servers'});
}]).factory("newTaskDetails", [function(){
  return {
    name : "",
    description : "",
    cbRequeueOnDead : true,
    selectedTask :"T-SQL",
    globalLimit : 1,
    localLimit : 1
  };
}]).run(['$rootScope', '$http', function($rootScope, $http){
  $rootScope.retrieveServers = function () {
    console.log('api/servers GET - Start');
    $http.get('api/servers').
    success(function (data) {
      console.log('api/servers GET - Completed');
      $rootScope.servers = data;
    }).
    error(function (data, status, headers, config) {
      console.log('api/servers GET - Error');
      $rootScope.servers = null;
    })};
  
  $rootScope.retrieveDeadExecutions = function() {
    console.log('api/deadexecutions GET - Start');
    $http.get('api/deadexecutions').
    success(function (data) {
      console.log('api/deadexecutions GET - Completed');
      $rootScope.deadexecutions = data;
    }).
    error(function (data, status, headers, config) {
      console.log('api/deadexecutions GET - Error');
      $rootScope.deadexecutions = null;
    })};

  $rootScope.retrieveLiveExecutions = function() {
    console.log('api/liveexecutions GET - Start');
    $http.get('api/liveexecutions').
    success(function (data) {
      console.log('api/liveexecutions GET - Completed');
      $rootScope.liveexecutions = data;
    }).
    error(function (data, status, headers, config) {
      console.log('api/liveexecutions GET - Error');
      $rootScope.liveexecutions = null;
    })};

  $rootScope.retrieveExecutionQueue = function() {
    console.log('api/queueitems GET - Start');
    $http.get('api/queueitems').
    success(function (data) {
      console.log('api/queueitems GET - Completed (' + data.length + ' elements)');
      $rootScope.executionQueue = data;
    }).
    error(function (data, status, headers, config) {
      console.log('api/queueitems GET - Error');
      $rootScope.executionQueue = null;
    })};

  $rootScope.retrieveServers();
  $rootScope.retrieveDeadExecutions();
  $rootScope.retrieveLiveExecutions();
  $rootScope.retrieveExecutionQueue();

  $rootScope.formatTaskConcurrency = function(concurrecy) {
    if(concurrecy == 0)
      return '∞';
    else
      return concurrecy;
  };

  $rootScope.initializeNewTaskDetails = function(newTaskDetails) {
    newTaskDetails.name = "";
    newTaskDetails.description = "";
    newTaskDetails.cbRequeueOnDead = true;
    newTaskDetails.selectedTask = "T-SQL";
    newTaskDetails.globalLimit = 1;
    newTaskDetails.localLimit = 1;
  };
}]);

