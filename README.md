# osu!framework-xr
 
An extension for [osu-framework (o!f)](https://github.com/ppy/osu-framework) which enables rendering and physics in 3d space. This project was originally a part of [osu!xr](https://github.com/Flutterish/osu-XR) (a VR port of [osu!lazer](https://github.com/ppy/osu)), but I deemed it useful for other creators, and as such it was split into its own thing.

## Rendering
o!f-xr is not a fork of o!f and only uses it as a dependency.
The OpenGL rendering design of o!f is not fit for 3D rendering and is very closed down, and as such
is was written from scratch. This allows far greater flexibility and both 
global and local performance issues.

The root component of o!f-xr is a `Scene`, which contains 3D drawables. It has a fully customizable 
rendering pipeline.

3D drawables have their own kind of draw node, which as opposed to 2D draw nodes, are flattened globally and sorted into render stages whose behaviour depends on the rendering pipeline. These draw nodes however, are similarly triple buffered. The triple buffer serves as a fast-lane udate thread to draw thread upload. 

Bigger uploads like mesh changes, and non-local uploads (say, changing a materials properties from the update thread) are handled using `IUpload`s, which are scheduled to execute on the draw thread.

The standard renderable "Model" consists of a Mesh, an optional Material and a VAO, however you are free
to create custom draw nodes that interact with your rendering pipeline in any way you want.

o!f-xr also provides a way to embed 2D drawables into the 3D scene with `Panel`s.

## Physics

## VR support

## Goals and non-goals
It is a **goal** of o!f-xr to allow all users to exploit it without any limits. 
This means that `internal`, as opposed to in o!f, is not allowed. The only allowable usage
of `internal` is with the `[Friend<T>]` attribute, as a `friend` access modifier. (TBD: analizer for this)

It is a **goal** of o!f-xr to provide easy to use and optimised code, such as a standard model class,
batching mechanisms, transparency sorting and cast/intersection physics (but not physics simulations).

It is a **non-goal** of o!f-xr to be a "3D game engine". While we will provide you with resuable,
generic code like o!f and a 3D object hierarchy with all the o!f luxuries, you will have to glue 
your game together. We will not provide any visual editor or a resource management system beside bare necessities.