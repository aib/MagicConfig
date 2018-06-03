# MagicConfig

MagicConfig is a .NET library for creating a hierarchy of change-aware classes with subscribable events.

## Classes

### ConfigItem
`ConfigItem` is the base class for all other classes. It has two methods; `Equals(ConfigItem)` and `Assign(ConfigItem)`. `Assign` updates the item with another item's values, generating events where appropriate. `Equals` is called first to determine whether an update is necessary in the first place. `Assign` can throw `ConfigItem.InvalidTypeAssignmentException` if an item is being assigned with an item of incompatible type or `null`.

### SingleItem
`SingleItem<T>` is a simple wrapper around the (reference) class `T`. It holds a reference to a `T` value and has implicit casts back and forth. (A `null` `SingleItem` converts to a `null` `T`.) `Assign` changes the reference and fires an `Update` event with both old and new references. `Equals` delegates to `T.Equals`.

### SingleValue
`SingleValue<T>` is the unrestricted version of `SingleItem`, designed for value types. It works the same as `SingleItem` except a `null` value converts to `default(T)` and `Equals` delegates to `object.Equals`.

### ItemList
`ItemList<T>` is a list of items of type `T`, which should be `IEquatable<T>`. `Assign` compares every existing element to every new element in a Cartesian fashion, firing `ItemAdded` and `ItemDeleted` events where appropriate. `Updated` is fired if any elements are added or deleted, or the order of the list changes. References to existing items are kept where possible, removals starting from the end of the list.

### StaticMap
`StaticMap<T>` is a wrapper around a type `T`; a strongly-typed map or dictionary, if you will. The type `T` itself should be derived from `StaticMap<T>` (think `class Foo: IEquatable<Foo>`). `StaticMap` detects the public members of `T` which are of the type `ConfigItem` using reflection. Only these fields are of interest to `StaticMap`. `Assign` updates these fields, firing `ItemUpdated` for individual items and `Updated` once, where appropriate.

### DynamicMap
`DynamicMap<T>` is an untyped map or dictionary from `string`s to `T`s. The type `T` should be derived from `ConfigItem`. Unlike `StaticMap`, `DynamicMap` does not inspect its instance and is not meant to be inherited by user classes. In this regard it is much more similar to a `Dictionary<string, ConfigItem>`. `Assign` uses the keys to determine whether an item needs to be added, updated or deleted, firing `ItemAdded`, `ItemUpdated` and `ItemDeleted`  where appropriate, as well as an overall `Updated` event.

### KeyedItemList
`KeyedItemList<T>` is a hack around configurations where a map was intended but a list was used. It is a list of items of type `T`, which should be derived from `ConfigItem` and implement `IKeyedItem`. `KeyedItemList` is very similar to `DynamicMap` (it is, in fact, a direct subclass) but uses an item's `GetKeyedItemKey` method to determine its key. Duplicate and dynamic keys are handled poorly.

## Examples ##

For usage examples please see the `UseCase#.cs` files under `tests/`.
