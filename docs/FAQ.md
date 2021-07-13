# Frequently Asked Questions

## Why Unity?

Unity offers ongoing support for WebGL with a [roadmap](https://portal.productboard.com/gupat5mdsl4luvs35fqy5vlq/tabs/60-web) containing multithreading, better mobile support and the new 3D webstandard WebGPU. The crossplatform nature of Unity allows distribution to other, native platforms (desktop, mobile etc.) in case specific features/use-cases have different system requirements, like a larger accessible memory range. 

## Why use tiles?

The total amount of unique objects and their geometry in a city or municipality is too much too fit in the memory limits of most platforms at once. The ongoing steps to add more objects and detail only keeps adding to this amount of data. The tile system splits this data up and makes sure that only geometry that is in view is loaded into the runtime memory, and the separation of different unique objects is only loaded on demand.

## Why 3D and not 2D?

Where an abstract 2D representation of data can give a better oversight or means to filter specific data, our 3D platform focusses more on the actual experience of data that is closer to real-life.

## Can I use Analytics?

Unity offers Unity Analytics that sends a fair amount of events/data by default when enabled.
Please refer to the Unity docs for their explanation on GDRP compliance of Unity Analytics:

https://docs.unity3d.com/Manual/UnityAnalyticsDataPrivacy.html

Our Netherlands3D.Analytics is an intermediate system that allows you to add your own event tracking API/service(s).
