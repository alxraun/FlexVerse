<!-- parents: @.frostline/graph/p-flexverse.xml, @.frostline/graph/p-flextools.xml, @.frostline/graph/p-flexverse-tooling.xml -->
# FlexVerse

A declarative reactive framework for RimWorld that includes a UI framework on top of Unity IMGUI.

## Runtime

* `Source/FlexVerse` -> `Behavior` + `FVPAttribute` + `Timeline` + `Widget` + `FVUI`
* `FlexVerseRoot` : `MonoBehaviour` -> `BehaviorRegistry` + `BehaviorUpdater`
* `FlexVerseRoot.Update` -> `BehaviorUpdater.Update` -> `Behavior.Update`
* `Behavior<T>` : `Behavior` -> `Value` + `Target` + `OnActivate` + `OnStart` + `OnUpdate` + `OnComplete` + `OnReset`
* `StaticBehavior<T>` + `TweenBehavior<T>` + `SpringBehavior<T>` + `KeyframeBehavior<T>` + `SequenceBehavior<T>` + `ComputedBehavior<T>` : `Behavior<T>`
* `EffectBehavior` + `TimelineBehavior` : `Behavior`
* `Select` + `Group.Combine` -> `ComputedBehavior`; `Behavior.OnUpdate` -> `ComputedBehavior` + `EffectBehavior`
* `Timeline` -> `ITimelineAction` -> `SequenceAction` | `ParallelAction` | `SetAction` | `CallAction` | `WaitAction` | `WaitUntilAction` | `IfAction` | `WhileAction`
* `SetAction` -> `IPropertyRef.Set` -> `Behavior.OnComplete`
* `Refs` -> `IPropertyRef`; `BindRef` -> `IPropertyRef.Changing`; `WatchRef` -> `IPropertyRef.Changed`
* `ReleaseFvp` -> `BehaviorRegistry.UnregisterSubscriptions` + `ReleaseBehaviors` -> `ReleaseBehavior` -> `BehaviorRegistry.Decrement` => `Behavior.Reset`

## Metaprogramming

* `Source/FlexVerse.SourceGenerators` -> `FlexVerseGenerator` -> `FvpProvider` | `WidgetProvider` -> `FvpEmitter` | `WidgetEmitter`
* `Source/FlexVerse.PropertyWeaver.Fody` -> `ModuleWeaver.Execute` -> `_Get{Prop}` + `_Set{Prop}`
* `FVPAttribute` -> `EnsureFvpInitialized` -> `ActivateBehavior` -> `BehaviorRegistry.Increment` + `Behavior.Activate`
* `_Set{Prop}` -> `SemEquals` -> `Changing` -> `Changed` -> `UnionWith` + `ActivateBehavior` + `ReleaseBehavior`
* `PropAttribute` + `StateAttribute` + `InheritedContextAttribute` : `FVPAttribute`
* `ApplyPropsFromDefinition` -> `PropAttribute`; `InitMembersFromDefinition` -> `PropAttribute` + `StateAttribute`
* `InheritedContextManager` -> `InitInheritedContext` + `_Update{Prop}FromAncestor`

## UI

* `FVUI.Render` -> `EngineRegistry.GetOrCreate` -> `WidgetEngine.ProcessUI`
* `WidgetEngine.ProcessUI` -> `FlexEventDispatcher.ProcessEvents` -> `WidgetReconciler` -> `WidgetLayoutCalculator` -> `WidgetGeometryBuilder.Refresh` -> `WidgetRenderer.RenderInstanceTree`
* `Widget.BuildRenderTree` -> `WidgetReconciler` -> `WidgetTree` -> `Widget.Key` + `Widget.OnMount` + `Widget.OnUnmount`
* `WidgetLayoutCalculator` -> `Flex.CalculateLayout` + `Widget.MeasureFunc` => `Rect`
* `WidgetGeometryBuilder` -> `WidgetGeometrySnapshot` + `Translate` + `Scale` + `Rotate` + `Overflow` + `ZIndex`
* `WidgetGeometrySnapshot` -> `TransformService` + `ClipService` + `WidgetHitTester` + `WidgetRenderer`
* `WidgetHitTester` + `FocusManager` -> `FlexEventDispatcher` -> `FlexEvent` -> `Widget.OnMouseDown` | `Widget.OnKeyDown`
* `WidgetRenderer` -> `Widget.OnGUI` + `Widget.BeginChildrenGUI` + `Widget.EndChildrenGUI` -> `UnityEngine.GUI`
* `View` + `Label` + `Image` + `Button` + `ScrollView` + `CanvasView` + `TextField` + `Checkbox` + `HorizontalSlider` + `AreaSlider` : `Widget`
* `View` + `Image` -> `Widgets/Drawers` + `GradientTextures`; `FlexVerseRoot.OnDestroy` -> `AppearanceMasks.DisposeCaches` + `GradientTextures.ClearCache`
* `FVUI.Release` -> `EngineRegistry.Release` -> `WidgetEngine.Shutdown` -> `WidgetTree.RemoveInstance`

## Repository

* `Source/FlexVerse/FlexVerse.csproj` -> `net48` + `Verse` + `UnityEngine`
* `Source/FlexVerse.Package` -> `build/FlexVerse.targets` + `FlexVerse.dll`
* `Source/FlexTools` -> `FlexToolsMod` -> `ToolManager` -> `Tool` -> `WidgetDebugger`
* `WidgetDebugger` -> `WidgetEngine` + `IPropertyRef` + `InspectorApp`
* `Source/Directory.Build.props` + `Source/Directory.Build.targets` -> `Source/MSBuild`
* `Scripts/{test,play}` -> `Scripts/_lib.py` -> `dotnet build` + `Docker`
* `Scripts/mods.json` + `Source/FlexVerse.Test` -> `.cache/{references,mods,staging,runs,pkg}`
* `Source/FlexVerse.Test/Xunit` + `Source/FlexVerse.Test/Pickle` + `Source/FlexVerse.Test/Explorer`
* `Scripts/test` -> `dotnet test` | `Pickle`; `Scripts/play` -> `Explorer`
* `FlexVerse.Log` -> `Player.log` -> `report.json`
* `.frostline` -> `p-*.xml` + `s-*.xml` + `ss-*.xml` + `m-*.xml`
