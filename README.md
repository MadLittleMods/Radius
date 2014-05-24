# Radius

A Complete Unity Reference Project 

This project is meant to be functionally impressive example a complete game. It is not made to be fun or interesting for a non-developer.

# Features
 - Full complete game
 - Multiplayer (Online([MasterServer](https://docs.unity3d.com/Documentation/ScriptReference/MasterServer.html)) and LAN): Semi-Authoratitive
 - Full Menu UI and HUD: Utilizing the awesome power of [Coherent UI](http://coherent-labs.com/); HTML, CSS, JS web stack
 	 - Server Browser
 	 - Match Lobby
 	 - Player Customization
	 - Options menu
	 - In-game HUD
 - Procedurally generated objects
 - Multiple Levels
 - Sound Effects and Music + Volume


# Details:
 - Developed in Unity 4.3.1f1
 - Tested with Coherent UI 1.8.1
 - [Requires Unity Pro(because Coherent UI uses native code)](http://docs.unity3d.com/Documentation/Manual/Plugins.html) to work in the editor. You can build with Unity Free and add the dlls manually. See the [guide I made on using unmanaged Dlls with Unity to get a hint on how to get Coherent UI working](http://ericeastwood.com/blog/17/unity-and-dlls-c-managed-and-c-unmanaged)

# Getting Started (Setup)

[![Radius: Getting Started Guide](http://i.imgur.com/Qz3Msr2.png)](http://www.youtube.com/watch?v=ehRKmvcdxFg)

 - Clone/Download the repo
 - Open in Unity
 - Import the Coherent UI package
 	 - You can [get a free evaluation version(only limitation is a watermark) of Coherent on their website](http://coherent-labs.com/)
 - Goto `Edit->Project Settings->Coherent UI->Select UI Folder` and select the `UIResources` folder in the Unity project root
 - You should be able to play it in the editor if you have Unity Pro
	 - If you have Unity Free, it will only work if you build the project and use [this guide](http://ericeastwood.com/blog/17/unity-and-dlls-c-managed-and-c-unmanaged) to manually copy over Coherent's Dlls



## Other features:

 - Character Controller/Driver: `CharacterDriver.cs`
 - Camera Controller (Conic): `ConicCameraController.cs`
 - Master Volume for Music and Soundeffects: `AudioManager.cs`, `AudioBase.cs`
 

# Changelog:

`0.2`:
 - Cleaned up `Assets/Scripts/ProceduralMeshes/` scripts
 - Added procedural tiling ground/floor `TextureTilingController.cs`
 - Profile box color initial color now is the color of that player
 - Added `UtilityMethods.cs`
 
`0.1`:
 - Initial code commited
 - Gameplay, UI, sound, networking

