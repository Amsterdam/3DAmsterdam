# quantized-mesh-tile-cs

[![NuGet Status](http://img.shields.io/nuget/v/quantized-mesh-tile.svg?style=flat)](https://www.nuget.org/packages/quantized-mesh-tile/)

A .NET library (netstandard2.0) for decoding a terrain tile (format quantized mesh).

More info about the quantized mesh format: https://github.com/AnalyticalGraphicsInc/quantized-mesh

For more awesome quantized mesh implementations see https://github.com/bertt/awesome-quantized-mesh-tiles

### Installation

```
PM> Install-Package quantized-mesh-tile
```

### Dependencies

NETStandard.Library 2.0.3 https://www.nuget.org/packages/NETStandard.Library/

### History

18-12-28: release version 0.3 with BinaryReader instead of FastBinaryReader

18-12-05: release version 0.2 with new .NET project file and conversion to WGS84 (method GetTriangles) removed.

### Usage

```
const string terrainTileUrl = @"https://maps.tilehosting.com/data/terrain-quantized-mesh/9/536/391.terrain?key=wYrAjVu6bV6ycoXliAPl";

var client = new HttpClient();
var bytes = client.GetByteArrayAsync(terrainTileUrl).Result;
var stream = new MemoryStream(bytes);

var terrainTile = TerrainTileParser.Parse(stream);
Console.WriteLine("Number of vertices: " + terrainTile.VertexData.vertexCount);
Console.ReadLine();
```

### Benchmark

```
BenchmarkDotNet=v0.10.1, OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-6820HQ CPU 2.70GHz, ProcessorCount=8
Frequency=10000000 Hz, Resolution=100.0000 ns, Timer=UNKNOWN
  [Host]     : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.7.3260.0
  DefaultJob : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.7.3260.0

Allocated=6.4 kB

                    Method |       Mean |    StdDev |
-------------------------- |----------- |---------- |
 ParseVectorTileFromStream | 76.5493 us | 1.6733 us |
 ```


 ### Sample: convert to GeoJSON

 See samples/qm2geojosn, sample code for converting a quantized mesh tile to GeoJSON. 
 
 Result: see https://github.com/bertt/quantized-mesh-tile-cs/blob/master/samples/qm2geojson/triangles.geojson
 
 Sample visualization:

![heightmap](https://user-images.githubusercontent.com/538812/66191533-dbddc700-e68e-11e9-8a62-e190353c8b90.png)

