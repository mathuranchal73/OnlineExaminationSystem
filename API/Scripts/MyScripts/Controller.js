app.controller('crudController', function ($scope, crudService) {

    $scope.IsNewRecord = 1; //The flag for the new record

    loadRecords();

    //Function to load all Employee records
    function loadRecords() {
        var promiseGet = crudService.getQuestions(); //The MEthod Call from service

        promiseGet.then(function (pl) { $scope.Question = pl.data },
            function (errorPl) {
                $log.error('failure loading Question', errorPl);
            });
    }



    //The Save scope method use to define the Employee object.
    //In this method if IsNewRecord is not zero then Update Employee else 
    //Create the Employee information to the server
    $scope.save = function () {
        var Question = {
            Id: $scope.id,
            QuestionCategoryId: $scope.QuestionCategoryId,
            QuestionType: $scope.QuestionType,
            Question1: $scope.Question1,
            ExhibitId: $scope.ExhibitId,
            Points: $scope.Points,
            isActive: $scope.isActive
        };
        //If the flag is 1 the it si new record
        if ($scope.IsNewRecord === 1) {
            var promisePost = crudService.post(Question);
            promisePost.then(function (pl) {
                $scope.id = pl.data.id;
                loadRecords();
            }, function (err) {
                console.log("Err" + err);
            });
        } else { //Else Edit the record
            var promisePut = crudService.put($scope.id, Question);
            promisePut.then(function (pl) {
                $scope.Message = "Updated Successfuly";
                loadRecords();
            }, function (err) {
                console.log("Err" + err);
            });
        }



    };

    //Method to Delete
    $scope.delete = function () {
        var promiseDelete = crudService.delete($scope.id);
        promiseDelete.then(function (pl) {
            $scope.Message = "Deleted Successfuly";
            $scope.id = 0;
            $scope.QuestionCategoryId = 0;
            $scope.QuestionType = 0;
            $scope.Question1 = "";
            $scope.ExhibitId = 0;
            $scope.Points = 0.0;
            $scope.isActive = 0;

            loadRecords();
        }, function (err) {
            console.log("Err" + err);
        });
    }

    //Method to Get Single Employee based on EmpNo
    $scope.get = function (que) {
        var promiseGetSingle = crudService.get(que.id);

        promiseGetSingle.then(function (pl) {
            var res = pl.data;
            $scope.id = res.id;
            $scope.QuestionCategoryId = res.QuestionCategoryId;
            $scope.QuestionType = res.QuestionType;
            $scope.Question1 = res.Question1;
            $scope.ExhibitId = res.ExhibitId;
            $scope.Points = res.Points;
            $scope.isActive = res.isActive;

            $scope.IsNewRecord = 0;
        },
            function (errorPl) {
                console.log('failure loading Question', errorPl);
            });
    }
    //Clear the Scopr models
    $scope.clear = function () {
        $scope.IsNewRecord = 1;
        $scope.id = 0;
        $scope.QuestionCategoryId = 0;
        $scope.QuestionType = 0;
        $scope.Question1 = "";
        $scope.ExhibitId = 0;
        $scope.Points = 0.0;
        $scope.isActive = 0;
    }
});