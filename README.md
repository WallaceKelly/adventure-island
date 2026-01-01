# Adventure Island

The original console game (circa 2006.)

<https://youtu.be/sbp8dXauV44>

## Build and run

Install .NET SDK 8.

Run `dotnet run -v m`

## Notes

The design includes:

* `WorldInfo` the info that is common to all players, including the definition of the areas.
* `UserInfo` the info about an individual player.
* `AreaInfo` includes the name, objects, and adjacent areas.
* `GameObjects` includes the object's name and the name of the verb that can be performed on it.
* `Verb` includes the name, the effect, and a virtual Execute method.
* `Effect` includes the name and a virtual Execute method; also an EffectDesign event and a DesignEffect method.

The idea is that the software `Executes` a `Verb`. The `Verb` can process the parameters, check conditions, etc. But the real work of the verb is done in an effect -- so that effects can be reused.

* `Program` executes `Verb`.
* `Verb` executes `Effect`.

When the `Verb` is constructed, it can set the value of `Verb.VerbEffect`. Or, the `Verb` can be designed to allow the user to select the `Effect` (e.g., the `VerbGet`.) In this case, the `Verb.Execute` method would check to see if `VerbEffect` is `null`. If it is `null`, you give the user the option of selecting the desired effect. Then, the `Effect` can be `DesignEffect`-ed.

The steps to adding a new verb...

1. Create a new class, deriving from `Verb`
2. Override the `Name` property's getter, to return the verb string
3. Do one/both of the following:
    * in the constructor, assign an Effect to the VerbEffect property
    * override the Execute method to perform the verb

The steps to adding an effect...

1. Create a new `[Serializable]` class, deriving from `Effect`.
2. Implement the `Name` property's getter, to return a string appropriate in the effects list.
3. Decorate the class with the `EffectAttribute`, optionally making the effect visible.
4. Implement the `Execute` method.
5. Optionally, update the `EffectsFactory` to attach an event handler to the effect's `EffectDesign` event.
