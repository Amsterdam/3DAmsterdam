# Frequently Asked Questions

## Why 3D and not 2D?

Where an abstract 2D representation of data can give a better oversight or means to filter specific data, a 3D view focusses more on the actual experience that is closer to a real-life experience.

## Can I use Analytics?

...

## Why Unity?

Unity offers ongoing support for WebGL with a [roadmap](https://portal.productboard.com/gupat5mdsl4luvs35fqy5vlq/tabs/60-web) containing multithreading, better mobile support and the new 3D webstandard WebGPU. The crossplatform nature of Unity allows distribution to native platforms (desktop, mobile etc.) if specific features/use-cases have different system requirements, like a larger accessible memory range. 

## Why use tiles?

The amount of unique objects and their geometry on a city or municipality scale is too much too fit in the memory limits of web- or mobile platforms. The ongoing steps to add more detail or objects only keeps adding to this amount of data. Therefore the tile system makes sure only the geometry that is in view is loaded into the runtime memory, and the separation of different unique objects is only loaded on demand