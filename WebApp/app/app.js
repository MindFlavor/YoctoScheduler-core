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

  //$rootScope.retrieveServers();
  $rootScope.retrieveDeadExecutions();
  $rootScope.retrieveLiveExecutions();
  $rootScope.retrieveExecutionQueue();

  // -----------------------
  // Servers
  // -----------------------
  $rootScope.areServersUpdating = false;
  $rootScope.servers = null;

  $rootScope.retrieveServers = function () {
    if($rootScope.areServersUpdating)
        return;

    $rootScope.areServersUpdating = true;

    console.log('api/servers GET - Start');

    $http.get('api/servers').
    then(function (data) {
      console.log('api/servers GET - Completed');
      $rootScope.servers = data.data;
      $rootScope.areServersUpdating = false;
    }).
    catch(function (error) {
      console.log('api/servers GET - Error');
      $rootScope.servers = null;
      $rootScope.areServersUpdating = false;
    })};

  $rootScope.findServerByID = function (serverID) {
    if(serverID == undefined || serverID == null)
        return null;

    if($rootScope.servers == null) {
      $rootScope.retrieveServers();
      return null;
    }

    for(var i=0; i<$rootScope.servers.length; i++) {
      if($rootScope.servers[i].ID == serverID)
          return $rootScope.servers[i];
    }

    $rootScope.retrieveServers();
    return null;
  };
  
  // -----------------------
  // Tasks
  // -----------------------
  $rootScope.areTasksUpdating = false;
  $rootScope.tasks = null;
  
  $rootScope.retrieveTasks = function () {
    if($rootScope.areTasksUpdating)
      return;

    $rootScope.areTasksUpdating = true;
    
    console.log('Retrieving tasks')
    $http.get('api/tasks').
    then(function (data) {
      console.log('Tasks retrieved from site')
      angular.forEach(data.data, function(elem) {
        elem.isEnabled = true;
      });
      $rootScope.tasks = data.data;
      $rootScope.areTasksUpdating = false;
    }).
    catch(function (data) {
      console.log('Tasks *not* retrieved')
      $rootScope.tasks = null;
      $rootScope.areTasksUpdating = true;
    });
  };

  $rootScope.findTaskByID = function (taskID) {
    if(taskID == undefined || taskID == null)
      return null;

    if($rootScope.tasks == null) {
      $rootScope.retrieveTasks();
      return null;
    }

    for(var i=0; i<$rootScope.tasks.length; i++) {
      if($rootScope.tasks[i].ID == taskID)
        return $rootScope.tasks[i];
    }

    $rootScope.retrieveTasks();
    return null;
  };

  // -----------------------
  // Schedules
  // -----------------------
  $rootScope.areSchedulesUpdating = false;
  $rootScope.schedules = null;

  $rootScope.retrieveSchedules = function () {
    if($rootScope.areSchedulesUpdating)
      return;

    $rootScope.areSchedulesUpdating = true;

    console.log('Retrieving schedules')
    $http.get('api/schedules').
    then(function (data) {
      console.log('schedules retrieved from site')
      angular.forEach(data.data, function(elem) {
        elem.style = "btn btn-danger enabled";
      });
      $rootScope.schedules = data.data;
      $rootScope.areSchedulesUpdating = false;
    }).
    catch(function (data) {
      console.log('schedules *not* retrieved')
      $rootScope.schedules = null;
      $rootScope.areSchedulesUpdating = true;
    });
  };

  $rootScope.findScheduleByID = function (scheduleID) {
    if(scheduleID == undefined || scheduleID == null)
      return null;

    if($rootScope.schedules == null) {
      $rootScope.retrieveSchedules();
      return null;
    }

    for(var i=0; i<$rootScope.schedules.length; i++) {
      if($rootScope.schedules[i].ID == scheduleID)
        return $rootScope.schedules[i];
    }

    $rootScope.retrieveSchedules();
    return null;
  };
  
  
  //////////////////////////////////////////////////
  // General purpose
  //////////////////////////////////////////////////
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

