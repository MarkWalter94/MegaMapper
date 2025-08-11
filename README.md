Everyone in their life at a certain point needed a mapping library.
Every mapping library is a mess to deal with, this is also a mess, but less.

This library was born to meet our needs to have asyncronus DI compatible mappings.
Have been using this for a while, publishing to the recent AutoMapper developments (v15 commercial licensing)


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

## Advanced example: using a service in an async mapping

The `AdvancedMappingProfile` example shows how to inject a service via Dependency Injection and use it inside an asynchronous mapping configuration.

**Profile Implementation**

```csharp
public class AdvancedMappingBuilder : MegaMapperMapBuilder<UserComplex, UserComplexDto>
{
    private readonly ICustomService _customService;

    //Every property is map with the base core mapping, then overrided with the properties defined here.
    public override bool UseBaseMap => true;

    public AdvancedMappingBuilder(ICustomService customService)
    {
        _customService = customService;

        MapField<string, string>(x => x.FirstName, y => y.FirstName, async (a, b, c) => await _customService.GetTheData());
    }
}
```

Inject the map:
```csharp
services.AddMegaMapperBuilder<AdvancedMappingBuilder>();
```

Then use the map

```csharp
var dto = await _mapper.Map<UserComplexDto>(user);
```

## Custom profiles 

If you need more flexibility you can also use a custom profile to define the entire logic:
```csharp
public class AdvancedMappingProfile : MegaMapperProfile<UserComplex, UserComplexDto>
{
    private readonly ICustomService _customService;
    public AdvancedMappingProfile(ICustomService customService)
    {
        _customService = customService;
    }
    protected override async Task<UserComplexDto> Map(UserComplex input, UserComplexDto output)
    {
        output.FirstName = await _customService.GetTheData();
        return output;
    }

    protected override Task<UserComplex> MapBack(UserComplexDto input, UserComplex output)
    {
        return Task.FromResult(output);
    }
}
```

Inject the map:
```csharp
services.AddMegaMapperProfile<AdvancedMappingProfile>();
```

Then use the map

```csharp
var dto = await _mapper.Map<UserComplexDto>(user);
```

## Define only the custom mapping for certain properties
In certains scenarios, you could have the need to custom-map only one property, then keep all the others
in a one to one mapping.
You can use a custom profile or custom bulder as above, and then override the property ```UseBaseMap```, 
if set to true, before applying any of the maps, the core proceed to apply the embedded automap.

## More examples

The repository contains comprehensive xUnit tests covering:

- Simple mapping between entities and DTOs
- Mapping with nested objects and collections
- Asynchronous mapping scenarios
- Type conversions
- Bidirectional mapping validation

You can watch these for examples.
