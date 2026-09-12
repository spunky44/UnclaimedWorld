# Unclaimed World
Contains the source code for Unclaimed World, a game published by Refactored Games.

Built using MonoGame.

For permissions and licensing information, see the LICENSE file.


# How to build Unclaimed World for the first time
The project relies on an old fork of MonoGame:
https://github.com/spunky44/MonoGame

Clone MonoGame next to the Unclaimed World solution folder

go to the MonoGame folder

run 

git config -f .gitmodules submodule.ThirdParty/NVorbis.url https://github.com/NVorbis/NVorbis.git
git submodule sync --recursive
Remove-Item -Recurse -Force .\ThirdParty\NVorbis -ErrorAction SilentlyContinue
git submodule update --init --recursive

.\Protobuild.exe --generate Windows
