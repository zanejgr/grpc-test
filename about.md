## About GRPC

GRPC was created by Google to use in their internal infractructure. It was
primarily designed for speedy server-to-server communication via HTTP/2,
in a language-agnostic way. Versus WCF, it is much faster, albiet less flexible.
It does not support message queuing directly, as WCF does.

## Protobufs
The main way to write a gRPC interface is via the Protobuf language.
This was created to fill the same functionality as XML, with less verbosity and
faster serialization. From the `.proto` file, fully-functional clients are
generated. Don't look at the DTO code
Officially supported languages are 
* C# 
* Javascript
* C/C++
* Python
* Ruby
* PHP
* Objective C
* Java

Unofficial bindings exist in nearly every language.

.NET also generates a server-side base service class.


### Hello World
A simple message can be defined like so:
```proto
syntax = "proto3";
package MyAppMessages;
option csharp_namespace = "MyApp.Namespace";

message A {
  string hello = 1;
}
```
The equivalent data contract in WCF would be
```cs
namespace MyApp.Namespace{
    [DataContract]
    public class A{
        [DataMember]
        public string Hello { get; set; }
    }
}
```
A service interface can use this message:
```proto
syntax = "proto3";
package MyAppServices;
option csharp_namespace = "MyApp.Namespace";

service ExampleService{
    rpc Echo (A) returns (A);
}
```
Which would be equivalent to the WCF interface
```cs
namespace MyApp.Namespace{
    [ServiceContract]
    public interface IExampleService{
        [OperationContract]
        A Echo(A a);
    }
}
```

### Practical Example:
* Protocol buffers can be imported and exported
* Enums are directly supported
* Messages can be nested
* Google provides a standard library of well-known types
```proto
syntax="proto3";

import "google/protobuf/any.proto";

package grpc_test.common;

enum Action{
    UNDEFINED_ACTION = 0;
    CREATED = 1;
    DELETED = 2;
}

message Event{
    Action action = 1;
    google.protobuf.Any data = 2;
}

enum ErrorCode {
    UNDEFINED_ERROR = 0;
    NOT_AUTHORIZED=1;
    FORBIDDEN=2;
    NOT_FOUND=3;
    BAD_REQUEST=4;
}
message Error{
    ErrorCode code = 1;
    string message = 2;
    google.protobuf.Any data = 3;
}
```

### More protobuf features:
* oneof: similar to a tagged union type
* repeated: array
```proto
syntax="proto3";
message Order{
    string name = 1;
    repeated Content content = 2;
}
message Content{
    oneof item{
        Burger burger = 1;
        Pizza pizza = 2;
    }
}
    enum Topping {
        CHEESE=1;
        PICKLE=2;
        LETTUCE=3;
        ONION=4;
        TOMATO=5;
    }
message Burger{
    i32 patties = 1;
    repeated Topping topping = 2;
}
message Pizza{
    enum Crust{
        THIN=0;
        THICK=1;
        REGULAR=2;
    }
    Crust crust = 1;
    repeated Topping topping = 2;
}
```
