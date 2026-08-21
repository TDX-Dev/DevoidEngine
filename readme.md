# Devoid Engine

Devoid Engine is a custom-built game engine developed as a learning project to explore game engine architecture, rendering systems and general application architecture. It is written in C# with .NET 9.

DevoidEngine uses its own RHI (DevoidGPU) to abstract any graphics backend calls, as such, there is no direct calls to any specific graphics backend in the engine project itself.

### Features

- Serialization of Scenes, GameObjects and Components

    - Serialization is done using MessagePack, but the serializers for each type are generated using a source generator, to avoid any runtime reflection. see `DevoidEngine.SourceGen`

- Postprocessing

    - Tonemapping (Filmic)
    - Bloom
    - Visibility Bitmask Ambient Occlusion

- Default Physically Based Rendering Shader

    - Following Google's Filament
        
        - Microfacet Cook-Torrance specular BRDF
        - Lambertian Diffuse BRDF
    
    - Split sum IBL Approximation with Spherical Harmonics probe for irradiance

- Asset Management pipeline

    - The asset management pipeline is part of the engine, for any loaded project, it scans all contents of the asset directory and checks for importers, it then imports the file accordingly and stores the engine equivalent type at EngineCache/

    - A meta file is stored next to any recognized file format, it contains relevant information like settings, timestamp and suhc. The modified timestamp is used to determine the reimport action of a file at startup.

    - All assets loaded are added to the asset cache, and are ref counted to release when the ref count reaches 0.

        - Although now that i think about it, it should wait some time to make sure cases like n-1 scene unloading and n scene loading does not cause cache misses. waiting would be better in this case TODO!

- Input System

    - All common input types are supported, and multiple types can be bound to one action.

    - Inputs are also routed first through InputLayers, to make sure events can be consumed before they reach the next level

        - For example, UI may want to consume the input before it reaches the playerscript for movement.

- UI System

    - The UI system uses a standard flexbox solver to place rects for layouting.

    - Font rendering is done via Sharpfont, it is then converted to signed distance fields and packed to an atlas using a skyline packer, this packed atlas is then saved to the disk as a engine font type in EngineCache/

