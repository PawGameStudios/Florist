# Project Rules

## Unity references

- Do not use `Find`, `FindObjectOfType`, `FindFirstObjectByType`, `FindAnyObjectByType`, `Transform.Find`, recursive hierarchy searches, or name-based hierarchy lookups.
- Do not use `GetComponent`, `TryGetComponent`, `GetComponentInChildren`, `GetComponentInParent`, or their plural variants in project gameplay/UI code.
- Scene, prefab, component, and hierarchy dependencies must be assigned explicitly with `[SerializeField]` references on the owning `MonoBehaviour`.
- Keep UI prefabs, target images, content roots, customer roots, scroll parents, buttons, and placeholder transforms on their scene/prefab controller. Do not move those references into gameplay balance/config ScriptableObjects.
- ScriptableObject configs contain data and balance values only. They must not be used as a service locator for scene or UI hierarchy references.
- Preserve existing serialized fields and prefab architecture unless the user explicitly asks to replace that architecture.

## Runtime flow

- Do not add `Update`, `LateUpdate`, or `FixedUpdate` polling loops for UI or gameplay state. Use explicit events, callbacks, page lifecycle methods, or tween completion callbacks.
- Do not construct production UI hierarchies at runtime with `new GameObject` or `AddComponent`. Build the hierarchy in prefabs and connect it through serialized references.
- Do not use `Resources.Load` for project gameplay or UI dependencies. Use explicit serialized prefab/asset references.

## Validation

- For every C# change, check likely compile errors, including namespaces, types, signatures, and Unity API compatibility.
- Do not launch Unity, Unity batch mode, tests, or player builds unless the user explicitly requests it.
- Do not build Unity-generated solution or project files with `dotnet`, `msbuild`, `csc`, or `nuget` unless the user explicitly requests it.
