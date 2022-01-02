# Slime Rancher Tweaks

This mod allows uers to tweak various things in Slime Rancher.

## Dependencies
* CMake 3.15+
* Visual Studio 2017+
* Slime Rancher with [SRML](https://www.nexusmods.com/slimerancher/mods/2) installed
* [AssemblyPublicizer](https://github.com/CabbageCrow/AssemblyPublicizer)

## Building
1. Create a file `Libs/GameLocation.txt` with the location of your Slime Rancher installation
2. Run Slime Rancher's `Assembly-CSharp.dll` through `AssemblyPublicizer` and place the resulting file here: `Libs/Assembly-CSharp.dll`
3. Create a `Build` folder in the root of this repo
4. From the `Build` folder, run `cmake ..` in a command prompt
5. Open the generated solution in Visual Studio and build the mod
