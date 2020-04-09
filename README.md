# Unity Telnet Server

Looking to make something communicate over a network without a fuss and don't care about security?
Well, do I have the tool for YOU!

This was built to communicate with Unity from a Telnet client (like PuTTY (https://www.putty.org/)) in order to do cool things over a network.
Like spawning cubes on unsuspecting Players!

### Version
1.0 - Tested across Mac and Windows

### Installation

While there are no dependencies, I would like to give a shoutout to @PimDeWitte(https://github.com/PimDeWitte) for their work on the
UnityMainThreadDispatcher.cs, which I use to send actions from an async thread to Unity's main thread.

1. Download the zip or clone into your project.
2. Drag the UnityTelnetServer prefab into your scene or add the UnityTelnetManager component to a gameobject.
3. Type in an available IP, Port, and your Commands and off you goooooooooooooo!

### Usage
```C#
	spawncube //Tells you amount of cubes in scene
	spawncube10 //Spawns 10 cubes into the scene and tells you it did
```

### Development
Feel free to submit a pull request and I'll take a look. But this is MIT-License so be cool ;)

### Author
Gir
https://github.com/GirtheGameDesigner