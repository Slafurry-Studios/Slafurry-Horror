# How to Use Utils and Gameplay Components

This document focuses on how to use each component in the utilities and interaction folders, especially from the Inspector and from code.

## 1. Interaction

### PlayerInspect

How to use it:
1. Add the PlayerInspect component to the player.
2. Fill in the following fields in the Inspector:
   - viewTransform: the player’s view point.
   - examinePoint: the point where the inspected object will be positioned.
   - firstPersonLook: the player’s FirstPersonLook component.
   - playerMovement: the player’s FirstPersonMovement component.
   - mainCamera: the main camera.
   - darkenOverlayCamera: an overlay camera for the darkening effect.
   - itemOverlayCamera: an overlay camera dedicated to the inspected object.
   - interactRange: the maximum interaction distance.
   - interactableMask: the layers that can be interacted with.
   - interactKey: the interaction key, default E.
   - promptText: UI text for the interaction prompt.
   - inspectPromptText: UI text for the inspect exit prompt.
   - examineTitleText and examineDescriptionText: UI for displaying object details.
3. Add the ExamineInfo component to the object that can be inspected.
4. Fill in the title and description in ExamineInfo.

How to call it from code:
```csharp
// Usually not called manually, because it activates when the player presses the interaction key.
// If needed, you can create a custom flow that calls StartInspect() from another script.
```

Notes:
- This component automatically starts inspection when the player gets close and presses the interaction key.
- While inspecting, gameplay is paused through PauseSystem.

### ExamineInfo

How to use it:
1. Add it to the object you want to inspect.
2. Fill in the title and description.

Example:
```csharp
public class ItemExample : MonoBehaviour
{
    [SerializeField] private ExamineInfo examineInfo;
}
```

---

## 2. UI Components

### UIButtonSFX

How to use it:
1. Add it to a GameObject that has a Button.
2. Fill in the category and effectName fields.
3. Make sure AudioSystem exists in the scene.

Example:
```csharp
// No manual call is required.
// Just attach it to the button and click it in Play Mode.
```

What happens:
- When the button is clicked, the component calls AudioSystem.SFX.PlaySFX2D(category, effectName).

### UITransition

How to use it:
1. Add it to a Canvas that has a full-screen Image.
2. Assign the fadeImage.
3. Set the fadeDuration.

How to call it from code:
```csharp
var transition = FindObjectOfType<UITransition>();
transition.FadeIn();
transition.FadeOut();
transition.InstantBlack();
transition.InstantClear();
```

### TextSequencer

How to use it:
1. Add it to a UI object that has a TMP_Text component.
2. Assign the textComponent.
3. Fill the texts array with the text for each part of the sequence.
4. Configure options such as useTypewriter, holdDuration, loop, and playOnStart.
5. If you want sound effects, fill in sfxId.

How to call it from code:
```csharp
var sequencer = FindObjectOfType<TextSequencer>();
sequencer.Play();
sequencer.Stop();
sequencer.SkipCutscene();
sequencer.ChangeScene("MainMenu");
```

### UICounter

How to use it:
1. Add it to an object that has a TMP_Text component.
2. Assign the label.
3. Set the format, for example N0 for thousand separators.

How to call it from code:
```csharp
var counter = FindObjectOfType<UICounter>();
counter.SetImmediate(100f);
counter.AnimateTo(1250f, 0.8f);
```

### UIFade

How to use it:
1. Add it to an object with a CanvasGroup.
2. Assign the canvasGroup.
3. Set the duration and other options.

How to call it from code:
```csharp
var fade = FindObjectOfType<UIFade>();
fade.FadeIn();
fade.FadeOut();
fade.SetImmediate(0f);
```

### UIScalePunch

How to use it:
1. Add it to a UI object.
2. Assign the target RectTransform if you want the effect on a specific element.
3. Set punchScale and duration.

How to call it from code:
```csharp
var punch = FindObjectOfType<UIScalePunch>();
punch.Play();
```

### UISlide

How to use it:
1. Add it to a UI panel.
2. Assign the rectTransform.
3. Set fromDirection, duration, delay, and playOnEnable.

How to call it from code:
```csharp
var slide = FindObjectOfType<UISlide>();
slide.SlideIn();
slide.SlideOut();
```

### UIDragHandler

How to use it:
1. Add it to a UI element that should be draggable.
2. Assign rootCanvas and canvasGroup.
3. When the drag ends, capture the OnDragEnded event to determine the drop target.

How to call it from code:
```csharp
var drag = FindObjectOfType<UIDragHandler>();
drag.OnDragStarted += () => Debug.Log("Drag start");
drag.OnDragEnded += eventData => Debug.Log("Drag end");
```

### UIBlink

How to use it:
1. Add it to an object with a CanvasGroup.
2. Set minAlpha, maxAlpha, and blinkSpeed.
3. If you want it to loop continuously, leave infinite enabled.

How to call it from code:
```csharp
var blink = FindObjectOfType<UIBlink>();
blink.Play();
blink.Stop();
```

### UISafeArea

How to use it:
1. Add it to the root UI panel you want to adapt to the safe area.
2. No fields need to be set.

What happens:
- The component automatically adjusts the anchors based on Screen.safeArea.

---

## 3. Trigger components

These are located in the gameplay trigger scripts folder and are meant to be used as event-driven helpers.

### AudioTrigger

File: [Assets/_Game/00_Scripts/Game/Triggers/AudioTrigger.cs](Assets/_Game/00_Scripts/Game/Triggers/AudioTrigger.cs)

How to use it:
1. Add it to a GameObject that should trigger audio when a certain event happens.
2. Fill in the sfxSource field if you want the sound to play from another object’s position.
3. Call one of the public methods through a Unity Event, Button OnClick, or another script.

Public methods:
- PlayMusic(string trackName)
- StopMusic(float fadeDuration)
- PlaySfx(string categoryKey)
- StopSFX(string categoryKey)
- StopAllSFX()

categoryKey format:
- Use the format `category:key` to call a specific effect.
- Use the format `category` to stop all effects in that category.

Example from code:
```csharp
var trigger = FindObjectOfType<AudioTrigger>();
trigger.PlayMusic("Gameplay");
trigger.PlaySfx("UI:Click");
trigger.StopSFX("UI:Click");
trigger.StopAllSFX();
```

### ObjectTrigger

File: [Assets/_Game/00_Scripts/Game/Triggers/ObjectTrigger.cs](Assets/_Game/00_Scripts/Game/Triggers/ObjectTrigger.cs)

How to use it:
1. Add the ObjectTrigger component to a collider GameObject.
2. Make sure the collider is a trigger.
3. Fill in enteredObjectTag with the tag of the object that is allowed to enter.
4. Add events in the UnityEvent actions field.

Example usage:
- Use it to trigger doors, cutscenes, animations, or sounds when the player enters a specific area.

Example code:
```csharp
// No manual call is required.
// Just add the event in the Inspector, for example to call a method on another script.
```

### When to use which trigger

- Use AudioTrigger when you want to play music, stop music, or play spatial SFX from an event.
- Use ObjectTrigger when you want to react to an object entering a trigger volume, such as opening a door, starting a cutscene, or firing a UnityEvent.
- Use AudioTrigger for audio-specific actions and ObjectTrigger for generic event-based interactions.

---

## 4. Game Feel Effects

### CameraShake

How to use it:
- Add it to a camera or object that should shake.
- Set the available parameters in the Inspector.
- Call the provided method from code when an event occurs.

Example:
```csharp
var shake = FindObjectOfType<CameraShake>();
shake.Play();
```

### ObjectShake

How to use it:
- Add it to a world object that should tremble.
- Call its play method when something like a hit or impact occurs.

### HitStop

How to use it:
- Add it to an object that should trigger a brief slow-motion effect.
- Call the available method when an impact occurs.

### Knockback

How to use it:
- Add it to an object that should be pushed backward.
- Call the method when an attack or explosion occurs.

### Blink

How to use it:
- Add it to a renderer or object that should blink briefly.
- Trigger it for hit feedback or damage effects.

### SquashObject

How to use it:
- Add it to a gameplay object that should use a squash/stretch effect.
- Trigger it for interaction feedback.

### Screenflash

How to use it:
- Add it to a camera or UI overlay object.
- Trigger it for dramatic events such as damage or explosions.

---

## 5. Pooling

### GenericPool<T>

How to use it:
1. Create a pool instance in a manager or system.
2. Pass the prefab and parent transform.
3. Use Get() to spawn and Release() to return the object to the pool.

Example:
```csharp
var pool = new GenericPool<EnemyController>(enemyPrefab, transform, 10, 50);
var enemy = pool.Get();
pool.Release(enemy);
```

### IPoolable

How to use it:
- Implement it on a prefab that needs its state reset when reused.
- Fill in OnSpawn() and OnDespawn() as needed.

### VFXCleaner

How to use it:
1. Add it to the root of a VFX prefab.
2. When the animation ends, call Clean() through an animation event or particle stop action.
3. If the VFX is spawned through VFXSystem, the pool will handle release automatically.

Example:
```csharp
var cleaner = FindObjectOfType<VFXCleaner>();
cleaner.Clean();
```

---

## 6. Practical tips

- For UI, you usually only need to attach the component and configure its fields in the Inspector.
- For gameplay feedback, call the method from a script event or collision handler.
- For VFX and pooling, make sure the prefab has a component compatible with the pool.
- If you want to reuse something across multiple scenes, avoid hardcoding references to a single object.
