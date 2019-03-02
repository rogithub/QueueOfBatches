# FeedAgent
This is the implementation of a client/server model for processing tasks in multithreaded batches.

It is implemented using .Net, and it is divided into two Visual Studio solutions:
  - /client/Client.sln
  - /server/Server.sln

### Client Part
It creates an executable console application that allows user to insert sample tasks to be executed later for the server application. Tasks will be inserted in an existing table (see /DBStructure folder).

### Server Part
It contains a console application that reads and executes tasks in multithreaded batches from the database (other sources are also possible described later on ITaskProvider topic).

## Tasks Components
This is completly generic, here are the main components.

  - TaskRunner. It is an Fsharp wrapper around MailboxProcessor class that is the core part of the server. It takes tasks in the form of ITask interface and execute them in multithreaded batches.
  - ITask. It consists of three methods Run, OnError, OnCancel. Run is the code that will be executed by TaskRunner, OnError and OnCancel will be called if required.
  - ITaskProvider is the source of tasks. It can be anything. In this code two examples are provided in the form of DbTaskProvider.cs which takes tasks from the database and is in the executable code and TaskProviderMock which is part of the unit testing on the server and it provides tasks from collections under System.Collections.Concurrent namespace.
