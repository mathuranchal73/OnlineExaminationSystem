app.service('crudService', function ($http) {


    //Create new record
    this.post = function (Question) {
        var request = $http({
            method: "post",
            url: "http://localhost:51415/api/Questions",
            data: Question
        });
        return request;
    }
    //Get Single Records
    this.get = function (id) {
        return $http.get("http://localhost:51415/api/Questions/" + id);
    }

    //Get All Employees
    this.getQuestions = function () {
        return $http.get("http://localhost:51415/api/Questions");
    }


    //Update the Record
    this.put = function (id, Question) {
        var request = $http({
            method: "put",
            url: "http://localhost:51415/api/Questions/" + id,
            data: Question
        });
        return request;
    }
    //Delete the Record
    this.delete = function (id) {
        var request = $http({
            method: "delete",
            url: "http://localhost:51415/api/Questions/" + id
        });
        return request;
    }

});