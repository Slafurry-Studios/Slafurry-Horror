# System Usage Guide

This folder contains the shared runtime systems used by the project: audio, input, loading, localization, pause, save, scene loading, and VFX. These systems are designed to be used through their public facade classes so gameplay and UI scripts do not need to depend on concrete implementations.

## 1. How the systems work

Most systems in this folder follow the same pattern:

- A static facade exposes the public API.
- A singleton-style system component provides the runtime implementation.
- The loading pipeline initializes registered objects in a controlled order.

That means the expected flow is:

1. Create or ensure the required system objects exist in the scene.
2. Assign the needed references in the Inspector.
3. Call the facade methods from gameplay, UI, or manager scripts.

## 2. Required setup in Unity

### Recommended boot scene
Create a boot scene or a persistent scene that contains the system objects:

- AudioSystem
- LoadingSystem
- LocalizationSystem
- PauseSystem
- SaveSystem
- SceneLoader
- VFXSystem
- InputHub

These objects should remain alive across scene transitions, so they should not be destroyed when loading a new scene.

---

## 3. Public API reference

### 3.1 Audio system

Namespace: Slafurry.System.Audio

Public facade:

```csharp
public static class Audio
{
    public static void PlayMusic(string trackName, float fade = 0.5f);
    public static void StopMusic(float fade = 0.5f);
    public static void PlaySFX2D(string category, string effect, bool loop = false);
    public static void PlaySFX3D(string category, string effect, Vector3 pos, bool loop = false);
    public static void StopSFX();
    public static void StopSFX(string category);
    public static void StopSFX(string category, string effect);
}
```

System implementation:

```csharp
public class AudioSystem : GameSystem<AudioSystem>
{
    public static MusicPlayer Music { get; }
    public static SFXPlayer SFX { get; }

    public event Action<float> OnMusicVolumeChanged;
    public event Action<float> OnSFXVolumeChanged;

    public void PlaySceneMusic();
    public void UpdateMusicVolume(float linearVolume);
    public void UpdateSFXVolume(float linearVolume);
}
```

Examples:

```csharp
using Slafurry.System.Audio;
using UnityEngine;

Audio.PlayMusic("MainMenu");
Audio.PlayMusic("Gameplay", 0.8f);
Audio.StopMusic();

Audio.PlaySFX2D("UI", "Click");
Audio.PlaySFX2D("UI", "Hover", loop: false);
Audio.PlaySFX3D("Enemy", "Hit", new Vector3(0, 1, 0));

Audio.StopSFX();
Audio.StopSFX("UI");
Audio.StopSFX("Enemy", "Hit");
```

#### Audio assets

- MusicData: ScriptableObject with an array of MusicTrack entries.
- MusicTrack fields:
  - trackName
  - clip
  - volume
- SFXData: ScriptableObject with categories and effects.
- SFXEffect fields:
  - groupID
  - clips
  - volume
  - maxSimultaneous
- SFXCategory fields:
  - categoryName
  - effects
  - poolSize

---

### 3.2 Input system

Namespace: Slafurry.System.InputHub

Current public facade:

```csharp
public static class Controls
{
    // Placeholder facade for future input bindings.
}
```

System implementation:

```csharp
public class InputHub : GameSystem<InputHub>
{
    [SerializeField] private InputActionAsset inputActions;
}
```

Current state:

- The input system is still scaffolded.
- The public entry points are not fully wired yet.
- When you implement input actions, expose them through the facade class instead of accessing InputHub directly.

Example placeholder:

```csharp
using Slafurry.System.InputHub;

// Add your bindings here once the input layer is implemented.
```

---

### 3.3 Loading system

Namespace: Slafurry.System

Public system:

```csharp
public class LoadingSystem : GameSystem<LoadingSystem>
{
    public event Action<float> OnProgressChanged;
    public event Action<string> OnStatusChanged;
    public event Action OnLoadingComplete;

    public void Register(IInitializable obj);
}
```

Examples:

```csharp
using Slafurry.System;
using Slafurry.Core.Interface;

LoadingSystem.Instance.OnProgressChanged += progress => Debug.Log(progress);
LoadingSystem.Instance.OnStatusChanged += status => Debug.Log(status);
LoadingSystem.Instance.OnLoadingComplete += () => Debug.Log("Loading done");

// Usually called automatically by Singleton/Manager base classes.
LoadingSystem.Instance.Register(myInitializableObject);
```

This is the central initialization gate for all objects that implement IInitializable.

---

### 3.4 Localization system

Namespace: Slafurry.System.Localization

Public facade:

```csharp
public static class Localize
{
    public static string Text(string key);
    public static void SetLanguage(string code);
}
```

System implementation:

```csharp
public class LocalizationSystem : GameSystem<LocalizationSystem>
{
    public string CurrentLanguage { get; }
    public event Action OnLanguageChanged;

    public string GetText(string key);
    public void SetLanguage(string languageCode);
}
```

Examples:

```csharp
using Slafurry.System.Localization;

string text = Localize.Text("menu.start");
Localize.SetLanguage("en");
Localize.SetLanguage("id");

LocalizationSystem.Instance.OnLanguageChanged += () => Debug.Log("Language changed");
```

The table format is defined by LocalizationTable:

```csharp
public class LocalizationTable : ScriptableObject
{
    public List<Entry> entries = new();
}
```

Each entry has:

- key
- en
- id

---

### 3.5 Pause system

Namespace: Slafurry.System.Pause

Public facade:

```csharp
public static class Pause
{
    public static void On(string key = "Global");
    public static void Off(string key = "Global");
    public static void Toggle(string key = "Global");
    public static bool IsPaused { get; }
    public static bool IsPausedBy(string key);
    public static void ForceResume();
}
```

System implementation:

```csharp
public class PauseSystem : GameSystem<PauseSystem>
{
    public bool IsPaused { get; }
    public event Action OnPaused;
    public event Action OnResumed;
    public event Action<string> OnPauseRequested;
    public event Action<string> OnPauseReleased;

    public void Pause(string key = "Global");
    public void Resume(string key = "Global");
    public void Toggle(string key = "Global");
    public bool IsPausedBy(string key);
    public void ForceResumeAll();
}
```

Examples:

```csharp
using Slafurry.System.Pause;

Pause.On("UI");
Pause.On("Cutscene");

if (Pause.IsPausedBy("UI"))
{
    Pause.Off("UI");
}

Pause.Toggle("Global");
Pause.ForceResume();
```

---

### 3.6 Save system

Namespace: Slafurry.System.Save

Public facade:

```csharp
public static class Save
{
    public static void To<T>(T data, string fileName);
    public static T From<T>(string fileName, T fallback = default);
    public static bool Exists(string fileName);
}
```

System implementation:

```csharp
public class SaveSystem : GameSystem<SaveSystem>
{
    public event Action<string> OnSaved;
    public event Action<string> OnLoaded;

    public void Save<T>(T data, string fileName);
    public T Load<T>(string fileName, T fallback = default);
    public bool HasSave(string fileName);
    public void DeleteSave(string fileName);
}
```

Examples:

```csharp
using Slafurry.System.Save;

var data = new PlayerProgressData();
Save.To(data, "player_progress");

var loaded = Save.From("player_progress", new PlayerProgressData());
bool hasSave = Save.Exists("player_progress");
```

Saved files are written as JSON into Application.persistentDataPath.

---

### 3.7 Scene loading system

Namespace: Slafurry.System.Scene

Public facade:

```csharp
public static class SceneSystem
{
    public static void Load(string sceneName);
}
```

System implementation:

```csharp
public class SceneLoader : GameSystem<SceneLoader>
{
    public event Action<string> OnSceneLoadStarted;
    public event Action<float> OnSceneLoadProgress;
    public event Action<string> OnSceneLoadCompleted;

    public void LoadScene(string sceneName);
}
```

Examples:

```csharp
using Slafurry.System.Scene;

SceneSystem.Load("MainMenu");
SceneSystem.Load("GameScene");

SceneLoader.Instance.OnSceneLoadStarted += scene => Debug.Log($"Loading {scene}");
SceneLoader.Instance.OnSceneLoadProgress += progress => Debug.Log(progress);
SceneLoader.Instance.OnSceneLoadCompleted += scene => Debug.Log($"Loaded {scene}");
```

---

### 3.8 VFX system

Namespace: Slafurry.System.VFX

Public facade:

```csharp
public static class VFX
{
    public static void Play(string key, Vector3 position);
    public static void Play(string key, Vector3 position, Quaternion rotation);
}
```

System implementation:

```csharp
public class VFXSystem : GameSystem<VFXSystem>
{
    public bool IsReady { get; }

    public VFXCleaner Play(string key, Vector3 position, Quaternion rotation = default);
}
```

Examples:

```csharp
using Slafurry.System.VFX;
using UnityEngine;

VFX.Play("Impact", transform.position);
VFX.Play("Explosion", transform.position, transform.rotation);
```

VFX entries are configured through VFXEntry:

```csharp
public class VFXEntry
{
    public string key;
    public VFXCleaner prefab;
    public int defaultCapacity = 5;
    public int maxSize = 30;
}
```

---

## 4. How to use the systems from scripts

### UI and gameplay scripts
Use the facade classes directly from other scripts:

```csharp
using Slafurry.System.Audio;
using Slafurry.System.Pause;
using Slafurry.System.Scene;
using Slafurry.System.Localization;
using Slafurry.System.VFX;

public class ExampleController : MonoBehaviour
{
    public void StartGame()
    {
        Audio.PlaySFX2D("UI", "Click");
        Pause.On("UI");
        Localize.SetLanguage("en");
        VFX.Play("Impact", transform.position);
        SceneSystem.Load("GameScene");
    }
}
```

### Managers and systems
If you are implementing a new manager, prefer inheriting from the shared base classes used by this project. That keeps initialization order consistent with the loading pipeline.

## 5. Notes and conventions

- Prefer the facade classes over direct access to concrete system components.
- Keep scene names, track names, and localization keys consistent with the data you assign in the Inspector.
- Use descriptive pause keys so multiple systems can pause the game safely.
- Subscribe to events only when necessary, and unsubscribe when the object is destroyed.
- For any new system, keep the public API explicit and document it in the same style.

## 6. Current implementation notes

Some parts of the systems are still scaffolded and may need finishing depending on your feature scope:

- InputHub is currently a placeholder for input binding setup.
- AudioSystem.LoadVolume is currently empty and can be completed later if you want volume settings to be restored automatically.
- SFXPlayer initialization logic is still incomplete in the current version and should be finished if you rely heavily on runtime SFX setup.

If you are extending this system, keep the public API consistent and add documentation for any new entry points.
