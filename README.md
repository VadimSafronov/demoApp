# Application for withdrawal feature implementation

The solution contains a .NET core library (Application) which should compile and run when you download it.

Project which is structured into the following 3 folders:
* Domain - this contains the domain models for a user and an account, and a notification service.
* Features - this contains two operations, one which is implemented (transfer money) and another which need to be implemented (withdraw money)
* DataAccess - this contains a repository for retrieving and saving an account (and the nested user it belongs to)


## The task

1. The task is to implement a money withdrawal in the WithdrawMoney.Execute(...) method in the features folder. 
   For consistency, the logic should be the same as the TransferMoney.Execute(...) method i.e. notifications for low funds and exceptions where the operation is not possible. 

2. As part of this process however, you should look to refactor some of the code in the TransferMoney.Execute(...) method into the domain models, and make these models to be less possible for misuse. 
   We're looking to make our domain models rich in behaviour and much more than just plain old objects, however we don't want any data persistance operations in our domain model (i.e. data access repositories). 
   This should simplify the task 1. of implementing WithdrawMoney.Execute(...).


## Guidelines

* You should aim to spend no more than 1 hour on this task
* You should not alter the notification service or the the account repository interfaces
* You may add unit/integration tests using a test framework (and/or mocking framework) of your choice
* You may edit this README.md if you want to give more details around your work (e.g. why you have done something a particular way, or anything else you would look to do but didn't have time)

Good luck!
