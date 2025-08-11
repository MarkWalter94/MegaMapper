Everyone in their life at a certain point needed a mapping library.
Every mapping library is a mess to deal with, this is also a mess, but less.

This library was born to meet our needs to have asyncronus DI compatible mappings, for example suppose this:



## Features

Everything you need from you mapper plus:
- Simple object-to-object mapping without declaring maps
- Asynchronous mapping functions with DI

## Usage

The project demonstrates how to configure the DI container to register the `MegaMapper` and your custom mapping profiles or builders.

Example of configuring DI in tests or your application:

```csharp
var services = new ServiceCollection();
services.AddMegaMapper();

//Optional custom builders:
services.AddMegaMapperBuilder<YourCustomMapBuilder>();
//Optional custom converters:
services.AddMegaMapperProfile<YourCustomProfile>();

```

You can then use the mapper to map objects asynchronously:

```csharp
var dto = await mapper.Map<YourDto>(yourEntity);
```
---

## More examples

The repository contains comprehensive xUnit tests covering:

- Simple mapping between entities and DTOs
- Mapping with nested objects and collections
- Asynchronous mapping scenarios
- Type conversions
- Bidirectional mapping validation

You can watch these for examples.