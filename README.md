# grpc-test

Trying out a GRPC full duplex solution with a net core host and a wpf client.
May add more later.

To run: `dotnet run` in the /Host/ directory.

# structure

The /proto/ directory is where services and messages are defined.

The /Host/ directory has an asp.net core 6 server.

The /Client/ directory has a WPF client, which will consume the API.

The /ConsoleClient/ directory has a console app for testing purposes.

The /ClientServiceLib/ directory is an empty classlib with necessary references
to generate C# classes from the protobufs, because WPF projects cannot include
this.

# more info

Protobufs are added to the Host and Client projects via the command
`dotnet grpc add-file ../proto/*`.
The corresponding classes will be generated at build-time.
