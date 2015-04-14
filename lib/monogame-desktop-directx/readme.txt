The monogame desktop setup references the a different set of directx libraries than that shipped with standard Monogame.

The DirectX libs in the DirectX11_2-net40 folder are used instead so the code can be injected with annotations and set data calls
for GPU debugging with Nvidia Nsight.

We are currently keeping a copy of these libraries here (C:\dev3\simsip-line\lib\monogame-desktop-directx) until
we can figure out git submodules which is where they will end up under.

For now, when setting up a new development ennvironment, after pulling MonoGame into position, copy over the folder Direct11_2-net40
into C:\dev3\cocos2d-xna\MonoGame\ThirdParty\Dependencies\SharpDX.