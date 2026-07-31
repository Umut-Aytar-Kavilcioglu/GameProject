# Project Overview

This repository contains a minimal 2D game framework written in modern C# and built on SDL3.

The framework aims to provide:

- A simple API inspired by Raylib
- A Unity-like GameObject and Component architecture
- Clear separation between game logic and rendering
- High performance without sacrificing readability
- A small and maintainable codebase
- A practical workflow for personal projects and small teams

The framework is primarily intended for personal projects and small teams rather than becoming a general-purpose commercial game engine.

---

# Core Principles

- Use modern C# and .NET.
- Use SDL3 as the low-level backend.
- Prefer composition over inheritance.
- Do not use ECS.
- Keep APIs small, explicit, and easy to understand.
- Prefer readability over cleverness.
- Avoid premature abstraction.
- Add systems only when there is a concrete need.
- Keep the folder and file structure minimal.
- Preserve room for future extension without implementing unused systems.

The preferred development strategy is:

> Build the smallest correct system that solves the current problem.

---

# Repository Boundaries

The repository is divided into two primary areas:

- `Framework`: Reusable engine and framework code.
- `Game`: Game-specific code that uses the framework.

Framework code must not depend on game-specific code.

Game code may depend on the framework.

Low-level SDL3 details should remain inside the framework and should not leak into normal gameplay code unless there is a strong technical reason.

Generated SDL binding files should not be manually modified unless the task explicitly concerns the bindings.

---

# Architectural Direction

The framework uses an object-and-component architecture.

The current ownership hierarchy is:

```text
Scene
    owns GameObjects

GameObject
    owns Components

Component
    belongs to at most one GameObject
```

The runtime supports one active scene and at most one pending scene transition.

Scenes are single-use runtime containers. A scene owns its game objects, exposes a camera, and may request a transition to another scene. Scene changes are applied by the engine at a controlled frame boundary.

Multiple simultaneously active scenes, scene stacks, overlays, and transition effects are not part of the current design.

---

# GameObject and Component Model

A `GameObject` represents an object in the game world and acts primarily as a component container.

Gameplay behavior should be introduced through components rather than through subclasses of `GameObject`.

General rules:

- Prefer component composition over inheritance.
- A component may belong to at most one game object.
- Component ownership must be managed by the framework.
- A game object may add or remove components at runtime.
- Runtime component mutation is part of the intended architecture.
- Component and game object ownership state must remain internally consistent.
- Avoid duplicating state that can be derived from another source of truth.
- Do not add lifecycle callbacks until their ordering and semantics are explicitly designed.

A prefab or factory system may create preconfigured game objects, but it should use the same normal component APIs as runtime code.

Prefab instances should remain normal, mutable runtime objects.

---

# Update Architecture

Not every component should be forced to participate in the game loop.

Components that require updates may eventually opt into update behavior through focused interfaces or another explicit registration mechanism.

Data-only components should not be required to implement empty lifecycle methods.

The update architecture must clearly define:

- Update ordering
- Active and inactive behavior
- Runtime component mutation during iteration
- Variable and fixed timestep responsibilities
- Registration and deregistration behavior

Do not introduce a full lifecycle system before these rules are deliberately decided.

Scene lifecycle methods are separate from component lifecycle. The engine currently invokes scene enter, update, and exit behavior, but components do not receive automatic lifecycle callbacks.

---

# Rendering Architecture

Rendering must remain separate from gameplay logic.

General rendering rules:

- Game objects do not render themselves.
- Gameplay components should not directly issue SDL rendering commands.
- Render-related components store rendering data.
- A centralized renderer reads game state and submits render commands.
- Batching and GPU submission belong to rendering-specific systems.
- Game state should be updated before rendering begins.
- SDL3 should remain hidden behind the framework rendering API.

Correctness and clarity should be established before advanced batching or rendering optimizations are introduced.

---

# Resource Ownership

Native and GPU resources require explicit ownership and deterministic cleanup.

General rules:

- Do not make every framework type disposable by default.
- Implement `IDisposable` only on types that actually own disposable or unmanaged resources.
- Resource ownership must be clear.
- A resource must not be released by multiple owners.
- High-level gameplay objects should not directly manage raw native handles.
- Avoid exposing native pointers through public APIs unless unavoidable.

Resource lifetime design should be completed before adding cleanup behavior to broad base classes.

---

# API Design

The public API should remain small and predictable.

Guidelines:

- Use properties for state.
- Use methods for meaningful operations.
- Do not add methods that merely duplicate simple property assignment.
- Use `internal` for framework implementation details.
- Use `protected` only for intentional inheritance extension points.
- Use `virtual` only when overriding is part of the design.
- Use `sealed` for concrete classes not designed for inheritance.
- Avoid unnecessary interfaces and base classes.
- Avoid global mutable state.
- Avoid service locator patterns.
- Avoid reflection-heavy automatic systems unless justified.
- Do not expose implementation details through public APIs.
- Prefer one clear way to perform an operation.

Public API changes should be deliberate because they may affect game code.

---

# C# Style

- Use modern C# where it improves clarity.
- Prefer classic constructors over primary constructors.
- Use file-scoped namespaces.
- Use nullable reference types correctly.
- Prefer explicit and descriptive names.
- Use PascalCase for types and public members.
- Use camelCase for parameters and local variables.
- Use guard clauses for invalid arguments.
- Prefer computed properties over duplicated mutable state.
- Avoid `null!` unless supported by a strong invariant.
- Do not suppress warnings instead of solving their cause.
- Keep methods focused.
- Keep classes centered around one responsibility.
- Preserve the existing code style unless a change is explicitly requested.

Compact syntax is acceptable only when it remains easy to understand.

---

# Performance Principles

Performance matters, but speculative optimization should be avoided.

Guidelines:

- Avoid unnecessary allocations in hot paths.
- Avoid repeated reflection during runtime loops.
- Avoid hidden expensive operations in simple-looking APIs.
- Measure before introducing complex optimizations.
- Prefer simple data flow and explicit ownership.
- Do not reduce readability for unproven performance gains.
- Optimize rendering, update, and resource systems only after their behavior is correct.

---

# Current Non-Goals

Do not implement the following unless explicitly requested:

- ECS
- Multiple simultaneously active scenes
- Scene stacks and overlays
- Animated scene transition systems
- Serialized prefab systems
- Nested prefabs
- Editor tooling
- Reflection-based automatic registration
- Dependency injection containers
- Complex event buses
- Transform hierarchies
- Job systems
- Multithreaded update systems
- Networking abstractions
- Plugin systems
- Scripting systems

These may become future systems, but they should not exist as placeholders.

---

# Future Direction

Potential future areas include:

- Rendering
- Input
- Audio
- Content and asset loading
- Camera systems
- Collision and physics
- Animation
- Tilemaps
- Lighting
- Render targets
- Save systems
- Prefabs
- Scene stacks and overlays
- Animated scene transitions
- Development tooling

These items describe possible future direction only.

They are not approved for implementation unless explicitly requested.

---

# Working Process

Before modifying code:

1. Read this file.
2. Inspect the relevant source files.
3. Understand the current implementation.
4. Identify the smallest useful change.
5. Explain significant architectural tradeoffs.

When modifying code:

- Make small and focused changes.
- Do not rewrite unrelated files.
- Do not introduce unrelated systems.
- Do not create new folders without clear value.
- Do not rename public APIs without approval.
- Preserve existing naming and formatting conventions.
- Do not treat planned systems as existing requirements.
- Do not change architecture silently.

After modifying code:

1. Run the project build.
2. Report compiler errors and warnings clearly.
3. Do not hide errors by disabling analyzers.
4. Summarize the modified files.
5. State any unresolved design decisions.

---

# Build

Run the solution build from the repository root:

```bash
dotnet build GameProject.slnx
```

Do not claim that the project builds successfully unless the command was actually run.

---

# Communication Rules

When discussing architecture:

- Distinguish current implementation from future direction.
- Do not present speculative ideas as decided behavior.
- Explain why a new abstraction is necessary.
- Prefer concrete tradeoffs over generic best practices.
- Avoid expanding the task beyond the request.
- Ask before making broad architectural changes.

When asked only to inspect or explain code:

- Do not modify files.
- Do not run destructive commands.
- Do not introduce additional systems.
