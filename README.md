![Consoled Logo](.readme/logo.png "Consoled Logo")

# Consoled - What is it?

Consoled lets you run commands from a simple terminal-esque window during the editor or runtime. It's straightforward to get started: slap an attribute on a static function and type its name in the window to run it.
Naturally, the asset has a lot more features:
- Parameter support
- Previous Commands
- Temporary Object Memory to assign and use objects as parameters
- Support for instance methods
- Support for properties and fields

## Upcoming Features

- Text suggestions
- Recursive Commands
- Simple stored procedures
- Command interruption
- Multiple windows
- Runtime IMGUI window
- Recurring execution (e.g. every 0.5s) -- useful to create monitoring windows
- Modules, for easily enabling or disabling groups of methods
- Command categories

## Please Note

This utility is still in very early stages of development. The core is there, and it works -- on the surface. Please report any bugs you find to harsh@aka.al or create a new issue.

# Table of Contents

- [Installation](#installation)
- [Quick Overview](#overview)
- [Contact](#contact)

# Installation <a name="installation"/>

Use the Unity Package Manager to download from this git url. This lets you fetch updates more easily in the future.

# Overview <a name="overview"/>

The Consoled editor window can be found under "Window > Tools > Consoled". You may open multiple windows in the editor. Each has its own instance.

For Runtime, currently only IMGUI is supported. Attach the script "Consoled IMGUI" to any gameobject. You can use gameobject's active state to control whether the window is shown or not.


# Contact<a name="contact"/>

Add a new issue, or drop me an e-mail at [harsh@aka.al](mailto:harsh@aka.al "Mail to harsh@aka.al"). If the topic is
this repository, put `PvCustomizer` somewhere in the subject.

I also have a no-effort blog at [https://hdeep.tech](https://hdeep.tech).

Finally, you can find me under `karmicto` in
the [Game Dev League Discord](https://discord.gg/eJbG9VD8R9 "GDL Discord Invite Link").