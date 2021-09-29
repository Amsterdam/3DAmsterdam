using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Netherlands3D.Utilities;
using Netherlands3D;
using Amsterdam3D.Sewerage;
using UnityEngine.Networking;
using System;
using UnityEngine.TestTools;
using System.IO;
using System.Linq;

public class GeoJsonTests
{
    string testStringAmsterdam = @"{""type"":""FeatureCollection"",""timeStamp"":""2021-05-10T15:44:28.734787+00:00"",""crs"":{""type"":""name"",""properties"":{""name"":""urn:ogc:def:crs:EPSG::4258""}},
  ""features"": [
    {""type"":""Feature"",""id"":""rioolleidingen.138075"",""geometry_name"":138075,""geometry"":{""type"":""LineString"",""coordinates"":[[4.90698478562009,52.3429529014738],[4.90702608778446,52.3428382055533]]},""properties"":{""vorm"":""Rond"",""objnr"":138075,""ggwcode"":""DX12"",""ggwnaam"":""De Pijp, Rivierenbuurt"",""diameter"":""297"",""wijkcode"":""K54"",""wijknaam"":""Rijnbuurt"",""buurtcode"":""K54b,K54a"",""buurtnaam"":""Rijnbuurt Oost,Kromme Mijdrechtbuurt"",""materiaal"":""Ultra 3"",""bob_eindpunt"":""-1.60"",""leidingnaam"":""131108(M50764 N51136)"",""type_leiding"":""HWA-Riool"",""type_stelsel"":""Regenwaterstelsel"",""bob_beginpunt"":""-1.00"",""stadsdeelcode"":""K"",""stadsdeelnaam"":""Zuid"",""type_fundering"":""Standaard"",""lengte_in_meters"":""13.07"",""bemalingsgebied"":null,""leiding_toelichting"":""Onbekend""}},
    {""type"":""Feature"",""id"":""rioolleidingen.138076"",""geometry_name"":138076,""geometry"":{""type"":""LineString"",""coordinates"":[[4.90698478562009,52.3429529014738],[4.90665251262403,52.3429023897671]]},""properties"":{""vorm"":""Rond"",""objnr"":138076,""ggwcode"":""DX12"",""ggwnaam"":""De Pijp, Rivierenbuurt"",""diameter"":""234"",""wijkcode"":""K54"",""wijknaam"":""Rijnbuurt"",""buurtcode"":""K54a"",""buurtnaam"":""Kromme Mijdrechtbuurt"",""materiaal"":""Ultra 3"",""bob_eindpunt"":""-1.10"",""leidingnaam"":""131109(M50764 N51270)"",""type_leiding"":""HWA-Riool"",""type_stelsel"":""Regenwaterstelsel"",""bob_beginpunt"":""-1.00"",""stadsdeelcode"":""K"",""stadsdeelnaam"":""Zuid"",""type_fundering"":""Standaard"",""lengte_in_meters"":""23.33"",""bemalingsgebied"":null,""leiding_toelichting"":""Onbekend""}}
  ],
""links"":[],""numberReturned"":1555,""numberMatched"":1555}";

    string testStringPdok = @"{
    ""type"": ""FeatureCollection"",
    ""numberMatched"": 523,
    ""name"": ""beheer_leiding"",
    ""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:EPSG::28992"" } },
    ""features"": [
    { ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""250724-250733-1"", ""uri"": ""lei18898-18907-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""250724"", ""uriB"": ""knp18898"", ""eindpunt"": ""250733"", ""uriE"": ""knp18907"", ""Begindatum"": ""2010-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": """", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rechthoekig"", ""BreedteLeiding"": ""6000"", ""HoogteLeiding"": ""350"", ""LengteLeiding"": ""9.43"", ""BobBeginpuntLeiding"": ""0.85"", ""BobEindpuntLeiding"": ""0.85"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 132803.42, 457145.83 ], [ 132798.02, 457153.57 ] ] } },
    { ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""25008F-250352-1"", ""uri"": ""lei18206-18505-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""25008F"", ""uriB"": ""knp18206"", ""eindpunt"": ""250352"", ""uriE"": ""knp18505"", ""Begindatum"": ""2008-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/PVC"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""150"", ""HoogteLeiding"": ""150"", ""LengteLeiding"": ""7.67"", ""BobBeginpuntLeiding"": ""-1.25"", ""BobEindpuntLeiding"": ""-1.25"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 132652.29, 457103.23 ], [ 132655.03, 457110.39 ] ] } }   
    ]
    }";

    string testStringAmsterdamCablesAndPipelineApi = @"{""type"":""FeatureCollection"",""timeStamp"":""2021-06-07T13:52:45.245595+00:00"",""crs"":{""type"":""name"",""properties"":{""name"":""urn:ogc:def:crs:EPSG::28992""}},
  ""features"": [
    {""type"":""Feature"",""id"":""ligging_lijn_totaal.2302136"",""geometry_name"":2302136,""geometry"":{""type"":""LineString"",""coordinates"":[[121096.56,487676.948],[121096.941,487676.732]]},""properties"":{""id"":2302136,""wkt"":""LINESTRING (121096.56 487676.948,121096.941 487676.732)"",""class"":""Kabelbed"",""thema"":""datatransport"",""diam_mm"":null,""diepte"":""-1"",""status"":""functional"",""voltage"":null,""beheerder"":""Ziggo B.V."",""buurtcode"":""A02b"",""buurtnaam"":""Leliegracht e.o."",""materiaal"":null}},
    {""type"":""Feature"",""id"":""ligging_lijn_totaal.2302137"",""geometry_name"":2302137,""geometry"":{""type"":""LineString"",""coordinates"":[[121096.56,487676.948],[121098.082,487679.648]]},""properties"":{""id"":2302137,""wkt"":""LINESTRING (121096.56 487676.948,121098.082 487679.648)"",""class"":""Kabelbed"",""thema"":""datatransport"",""diam_mm"":null,""diepte"":""-1"",""status"":""functional"",""voltage"":null,""beheerder"":""Ziggo B.V."",""buurtcode"":""A02b"",""buurtnaam"":""Leliegracht e.o."",""materiaal"":null}},
    {""type"":""Feature"",""id"":""ligging_lijn_totaal.2302143"",""geometry_name"":2302143,""geometry"":{""type"":""LineString"",""coordinates"":[[121096.591,487223.251],[121101.863,487222.733]]},""properties"":{""id"":2302143,""wkt"":""LINESTRING (121096.591 487223.251,121101.863 487222.733)"",""class"":""OlieGasChemicalienPijpleiding"",""thema"":""gas lage druk"",""diam_mm"":""170"",""diepte"":""-1"",""status"":""functional"",""voltage"":null,""beheerder"":""Liander N.V. Pac 2Ai68G2"",""buurtcode"":""A01f"",""buurtnaam"":""Spuistraat Zuid"",""materiaal"":""grijsGietijzer""}}
  ],
""links"":[{""href"":""https://api.data.amsterdam.nl/v1/wfs/leidingeninfrastructuur/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&TYPENAMES=ligging_lijn_totaal&OUTPUTFORMAT=application%2Fjson&BBOX=121000%2C487000%2C122000%2C488000&COUNT=3&STARTINDEX=9523"",""rel"":""next"",""type"":""application/geo+json"",""title"":""next page""},{""href"":""https://api.data.amsterdam.nl/v1/wfs/leidingeninfrastructuur/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&TYPENAMES=ligging_lijn_totaal&OUTPUTFORMAT=application%2Fjson&BBOX=121000%2C487000%2C122000%2C488000&COUNT=3&STARTINDEX=9517"",""rel"":""previous"",""type"":""application/geo+json"",""title"":""previous page""}],""numberReturned"":3,""numberMatched"":117014}";


    string testStringManholesAmsterdam = @"{""type"":""FeatureCollection"",""timeStamp"":""2021-05-10T14:45:47.524395+00:00"",""crs"":{""type"":""name"",""properties"":{""name"":""urn:ogc:def:crs:EPSG::4258""}},
  ""features"": [
    {""type"":""Feature"",""id"":""rioolknopen.130052"",""geometry_name"":""M69346"",""geometry"":{""type"":""Point"",""coordinates"":[4.91561158208928,52.342962335247]},""properties"":{""objnr"":130052,""ggwcode"":""DX15"",""ggwnaam"":""Watergraafsmeer"",""wijkcode"":""M58"",""wijknaam"":""Omval/Overamstel"",""buurtcode"":""M58h"",""buurtnaam"":""De Omval"",""knoopnummer"":""M69346"",""objectsoort"":""Knikpunt"",""symbool_hoek"":""0E-10"",""x_coordinaat"":""122862.9900000000"",""y_coordinaat"":""483998.4400000000"",""gemaal_nummer"":null,""stadsdeelcode"":""M"",""stadsdeelnaam"":""Oost"",""type_fundering"":null,""overstort_nummer"":null,""putdekselhoogte"":""0.20000000"",""overstort_drempelbreedte"":0,""drempelhoogte_overstortputten"":""0.0000000000""}},
    {""type"":""Feature"",""id"":""rioolknopen.140300"",""geometry_name"":""N50001"",""geometry"":{""type"":""Point"",""coordinates"":[4.90876406244186,52.3425051475584]},""properties"":{""objnr"":140300,""ggwcode"":""DX12"",""ggwnaam"":""De Pijp, Rivierenbuurt"",""wijkcode"":""K54"",""wijknaam"":""Rijnbuurt"",""buurtcode"":""K54b"",""buurtnaam"":""Rijnbuurt Oost"",""knoopnummer"":""N50001"",""objectsoort"":""Regenwaterrioolput"",""symbool_hoek"":""-12.4300000000"",""x_coordinaat"":""122396.0300000000"",""y_coordinaat"":""483950.6300000000"",""gemaal_nummer"":null,""stadsdeelcode"":""K"",""stadsdeelnaam"":""Zuid"",""type_fundering"":""Standaard"",""overstort_nummer"":null,""putdekselhoogte"":""0.68000000"",""overstort_drempelbreedte"":0,""drempelhoogte_overstortputten"":""0.0000000000""}}
   
  ],
""links"":[],""numberReturned"":1466,""numberMatched"":1466}";

    string testStringManholesPdok = @"{
    ""type"": ""FeatureCollection"",
    ""numberMatched"": 457,
    ""name"": ""beheer_put"",
    ""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:EPSG::28992"" } },
    ""features"": [
    { ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": ""neutronweg"", ""stelseltype"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/GemengdStelsel"", ""naam"": ""120034"", ""uri"": ""knp4378"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Inspectieput"", ""Begindatum"": """", ""Einddatum"": """", ""Maaiveldhoogte"": ""1.530"", ""MateriaalPut"": """", ""VormPut"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedtePut"": ""0"", ""LengtePut"": ""0"", ""HoogtePut"": """", ""Wanddikte"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""AantalWoningen"": ""0"", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": """", ""BergendOppervlak"": """" }, ""geometry"": { ""type"": ""Point"", ""coordinates"": [ 132303.35, 457990.35 ] } },
    { ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": ""neutronweg"", ""stelseltype"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/GemengdStelsel"", ""naam"": ""120035"", ""uri"": ""knp4379"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Inspectieput"", ""Begindatum"": """", ""Einddatum"": """", ""Maaiveldhoogte"": ""1.540"", ""MateriaalPut"": """", ""VormPut"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedtePut"": ""0"", ""LengtePut"": ""0"", ""HoogtePut"": """", ""Wanddikte"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""AantalWoningen"": ""0"", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": """", ""BergendOppervlak"": """" }, ""geometry"": { ""type"": ""Point"", ""coordinates"": [ 132347.73, 457958.06 ] } }
    
    ]
    }";

    string testStringPdokEmpty = @"{
""type"": ""FeatureCollection"",
""name"": ""beheer_leiding"",
""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:EPSG::28992"" } },
""features"": [

]
}
";

    string testStringPdokOne = @"{
""type"": ""FeatureCollection"",
""numberMatched"": 1,
""name"": ""beheer_leiding"",
""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:EPSG::28992"" } },
""features"": [
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""6194U-6195U-1"", ""uri"": ""lei42297-42298-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Duiker"", ""beginpunt"": ""6194U"", ""uriB"": ""knp42297"", ""eindpunt"": ""6195U"", ""uriE"": ""knp42298"", ""Begindatum"": """", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Staal"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""800"", ""HoogteLeiding"": ""800"", ""LengteLeiding"": ""6.82"", ""BobBeginpuntLeiding"": ""0.06"", ""BobEindpuntLeiding"": ""0.05"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 139074.18, 452285.67 ], [ 139080.57, 452288.05 ] ] } }
]
}
";

    string testStringAmsterdamOne = @"{""type"":""FeatureCollection"",""timeStamp"":""2021-05-10T15:44:28.734787+00:00"",""crs"":{""type"":""name"",""properties"":{""name"":""urn:ogc:def:crs:EPSG::4258""}},
  ""features"": [
    {""type"":""Feature"",""id"":""rioolleidingen.138075"",""geometry_name"":138075,""geometry"":{""type"":""LineString"",""coordinates"":[[4.90698478562009,52.3429529014738],[4.90702608778446,52.3428382055533]]},""properties"":{""vorm"":""Rond"",""objnr"":138075,""ggwcode"":""DX12"",""ggwnaam"":""De Pijp, Rivierenbuurt"",""diameter"":""297"",""wijkcode"":""K54"",""wijknaam"":""Rijnbuurt"",""buurtcode"":""K54b,K54a"",""buurtnaam"":""Rijnbuurt Oost,Kromme Mijdrechtbuurt"",""materiaal"":""Ultra 3"",""bob_eindpunt"":""-1.60"",""leidingnaam"":""131108(M50764 N51136)"",""type_leiding"":""HWA-Riool"",""type_stelsel"":""Regenwaterstelsel"",""bob_beginpunt"":""-1.00"",""stadsdeelcode"":""K"",""stadsdeelnaam"":""Zuid"",""type_fundering"":""Standaard"",""lengte_in_meters"":""13.07"",""bemalingsgebied"":null,""leiding_toelichting"":""Onbekend""}}    
  ],
""links"":[],""numberReturned"":1555,""numberMatched"":1555}";

    string testStringMultipleSegmentPdok = @"{
""type"": ""FeatureCollection"",
""name"": ""beheer_leiding"",
""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:EPSG::28992"" } },
""features"": [
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""320999-321000-1"", ""uri"": ""lei28390-28393-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""320999"", ""uriB"": ""knp28390"", ""eindpunt"": ""321000"", ""uriE"": ""knp28393"", ""Begindatum"": ""2006-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/PVC"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""125"", ""HoogteLeiding"": ""125"", ""LengteLeiding"": ""69.34"", ""BobBeginpuntLeiding"": ""-1.32"", ""BobEindpuntLeiding"": ""-1.29"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 128714.08, 456715.91 ], [ 128726.92, 456784.05 ] ] } },
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""321049-321050-1"", ""uri"": ""lei28449-28452-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""321049"", ""uriB"": ""knp28449"", ""eindpunt"": ""321050"", ""uriE"": ""knp28452"", ""Begindatum"": ""2009-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/PVC"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""200"", ""HoogteLeiding"": ""200"", ""LengteLeiding"": ""65.88"", ""BobBeginpuntLeiding"": ""-0.77"", ""BobEindpuntLeiding"": ""-0.83"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 129029.42, 456653.39 ], [ 129029.21, 456654.99 ], [ 129022.98, 456662.79 ], [ 129019.19, 456668.19 ], [ 129014.61, 456674.81 ], [ 129009.08, 456683.96 ], [ 129004.43, 456692.62 ], [ 129001.15, 456699.63 ], [ 128997.08, 456708.99 ], [ 128995.88, 456709.91 ] ] } },
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""321050-321051-1"", ""uri"": ""lei28452-28453-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""321050"", ""uriB"": ""knp28452"", ""eindpunt"": ""321051"", ""uriE"": ""knp28453"", ""Begindatum"": ""2009-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Beton"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""300"", ""HoogteLeiding"": ""300"", ""LengteLeiding"": ""16.31"", ""BobBeginpuntLeiding"": ""-1.15"", ""BobEindpuntLeiding"": ""-1.17"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 128995.88, 456709.91 ], [ 128980.46, 456704.58 ] ] } },
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""321050-321052-1"", ""uri"": ""lei28452-28454-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""321050"", ""uriB"": ""knp28452"", ""eindpunt"": ""321052"", ""uriE"": ""knp28454"", ""Begindatum"": ""2009-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/PVC"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""200"", ""HoogteLeiding"": ""200"", ""LengteLeiding"": ""2.09"", ""BobBeginpuntLeiding"": ""-0.83"", ""BobEindpuntLeiding"": ""-0.82"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 128995.88, 456709.91 ], [ 128997.58, 456711.12 ] ] } },
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""321052-321053-1"", ""uri"": ""lei28454-28455-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""321052"", ""uriB"": ""knp28454"", ""eindpunt"": ""321053"", ""uriE"": ""knp28455"", ""Begindatum"": ""2009-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/PVC"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""200"", ""HoogteLeiding"": ""200"", ""LengteLeiding"": ""15.79"", ""BobBeginpuntLeiding"": ""-0.82"", ""BobEindpuntLeiding"": ""-0.81"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 128997.58, 456711.12 ], [ 128990.83, 456725.09 ] ] } },
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""321053-321054-1"", ""uri"": ""lei28455-28456-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""321053"", ""uriB"": ""knp28455"", ""eindpunt"": ""321054"", ""uriE"": ""knp28456"", ""Begindatum"": ""2009-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/PVC"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""200"", ""HoogteLeiding"": ""200"", ""LengteLeiding"": ""55.61"", ""BobBeginpuntLeiding"": ""-0.81"", ""BobEindpuntLeiding"": ""-0.75"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 128990.83, 456725.09 ], [ 128991.41, 456725.94 ], [ 128988.92, 456735.47 ], [ 128987.33, 456742.67 ], [ 128985.99, 456750.57 ], [ 128984.92, 456759.02 ], [ 128984.1, 456769.15 ], [ 128983.77, 456779.09 ], [ 128983.11, 456780.0 ] ] } },
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""321054-321055-1"", ""uri"": ""lei28456-28457-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""321054"", ""uriB"": ""knp28456"", ""eindpunt"": ""321055"", ""uriE"": ""knp28457"", ""Begindatum"": ""2009-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/PVC"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""200"", ""HoogteLeiding"": ""200"", ""LengteLeiding"": ""47.13"", ""BobBeginpuntLeiding"": ""-0.75"", ""BobEindpuntLeiding"": ""-0.7"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 128983.11, 456780.0 ], [ 128983.73, 456780.61 ], [ 128984.32, 456794.93 ], [ 128984.77, 456799.57 ], [ 128986.18, 456809.58 ], [ 128989.48, 456826.53 ], [ 128988.61, 456826.73 ] ] } },
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""321055-321056-1"", ""uri"": ""lei28457-28458-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""321055"", ""uriB"": ""knp28457"", ""eindpunt"": ""321056"", ""uriE"": ""knp28458"", ""Begindatum"": ""2009-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/PVC"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""200"", ""HoogteLeiding"": ""200"", ""LengteLeiding"": ""13.56"", ""BobBeginpuntLeiding"": ""-0.7"", ""BobEindpuntLeiding"": ""-0.69"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 128988.61, 456826.73 ], [ 128991.83, 456838.23 ] ] } },
{ ""type"": ""Feature"", ""properties"": { ""Dataset"": ""Utrecht"", ""Stelsel"": """", ""stelseltype"": """", ""naam"": ""321056-321057-1"", ""uri"": ""lei28458-28459-1"", ""type"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Drain"", ""beginpunt"": ""321056"", ""uriB"": ""knp28458"", ""eindpunt"": ""321057"", ""uriE"": ""knp28459"", ""Begindatum"": ""2009-01-01"", ""Einddatum"": """", ""MateriaalLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/PVC"", ""VormLeiding"": ""http:\/\/data.gwsw.nl\/1.4\/totaal\/Rond"", ""BreedteLeiding"": ""200"", ""HoogteLeiding"": ""200"", ""LengteLeiding"": ""9.58"", ""BobBeginpuntLeiding"": ""-0.69"", ""BobEindpuntLeiding"": ""-0.66"", ""Wanddikte"": """", ""Verbindingstype"": """", ""Fundering"": """", ""DatumInspectie"": """", ""DatumReiniging"": """", ""WIONThema"": """", ""AantalWoningen"": """", ""Aantal_ieBedrijven"": """", ""Aantal_ieRecreatie"": """", ""AfvoerendOppervlak"": ""0"" }, ""geometry"": { ""type"": ""LineString"", ""coordinates"": [ [ 128991.83, 456838.23 ], [ 128992.71, 456837.92 ], [ 128995.19, 456845.78 ], [ 128994.93, 456847.3 ] ] } }
]
}
";

    string testStringBuurtnamen = "{\"type\":\"FeatureCollection\",\"totalFeatures\":10,\"features\":[{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3823\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[137944.579,456265.4921],[137914.261,456184.5311],[137886.6799,456117.5131],[137853.3629,456053.1531],[137814.5629,455991.9421],[137770.5749,455934.3461],[137749.0999,455909.6401],[137660.8649,455817.8901],[137655.2249,455812.3761],[137620.1129,455777.5591],[137596.1379,455753.5951],[137573.6109,455729.3471],[137554.6339,455706.3151],[137537.7349,455681.7161],[137519.2839,455648.1201],[137499.8989,455605.1721],[137436.855,455456.942],[137422.067,455416.867],[137410.827,455375.6551],[137405.761,455350.1501],[137399.517,455310.3311],[137397.364,455286.4671],[137397.021,455278.3401],[137388.9469,455278.4801],[137380.3229,455278.0131],[137372.1649,455276.1481],[137363.7739,455273.1161],[137357.1319,455270.0021],[137352.61,455277.634],[137346.466,455288.717],[137324.782,455326.423],[137318.518,455338.71],[137314.663,455347.625],[137311.772,455355.816],[137308.881,455365.935],[137302.617,455396.534],[137290.571,455457.972],[137268.405,455555.789],[137267.381,455561.091],[137266.658,455568.318],[137266.658,455573.137],[137267.14,455577.956],[137268.224,455583.3771],[137269.911,455589.159],[137273.104,455597.471],[137277.0789,455605.6621],[137280.9339,455612.1671],[137290.2099,455625.7801],[137292.4979,455628.5501],[137314.2429,455653.9691],[137319.0609,455660.7151],[137327.1329,455673.6051],[137329.7229,455678.7851],[137332.433,455685.5311],[137334.541,455691.554],[137335.565,455696.011],[137336.288,455704.805],[137335.565,455712.876],[137334.06,455717.334],[137332.192,455721.79],[137330.144,455725.404],[137325.627,455732.031],[137322.796,455735.764],[137320.086,455738.536],[137314.184,455743.594],[137307.859,455748.534],[137300.812,455753.713],[137292.921,455759.135],[137276.538,455769.736],[137259.191,455780.337],[137243.289,455790.456],[137234.134,455797.202],[137229.557,455801.297],[137225.942,455805.394],[137222.45,455810.452],[137219.678,455815.995],[137217.149,455823.101],[137214.499,455832.497],[137211.486,455844.906],[137208.113,455861.29],[137198.476,455917.667],[137194.485,455938.3031],[137176.552,456031.024],[137175.527,456039.456],[137175.286,456043.551],[137175.286,456047.406],[137176.371,456054.032],[137177.154,456057.044],[137178.659,456061.139],[137180.5269,456064.9951],[137183.9599,456070.2951],[137204.0589,456097.5831],[137244.9159,456153.0541],[137247.9879,456157.8721],[137250.277,456162.4491],[137252.084,456167.388],[137253.048,456172.448],[137252.807,456177.387],[137251.361,456182.808],[137248.831,456187.265],[137245.338,456191.722],[137240.069,456196.891],[137233.653,456202.082],[137226.184,456207.386],[137183.785,456235.198],[137165.116,456248.058],[137155.09,456255.665],[137147.622,456262.303],[137141.813,456268.11],[137137.941,456272.398],[137134.345,456276.961],[137131.856,456280.833],[137127.984,456288.024],[137125.633,456293.832],[137124.112,456298.673],[137121.899,456308.491],[137119.134,456325.086],[137117.416,456339.2041],[137529.9709,456378.1941],[137659.1959,456388.5921],[137856.7469,456402.7021],[137905.7639,456404.1881],[137927.137,456404.299],[137994.7159,456404.6501],[137944.579,456265.4921]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Buiten Wittevrouwen\"}},{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3824\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[137955.6589,456253.2261],[137974.6229,456225.9821],[138022.8839,456158.7241],[138033.5809,456142.4261],[138040.024,456123.8171],[138050.965,456079.3021],[138062.149,456025.7861],[138090.2279,456009.9201],[138097.0369,456005.9151],[138107.4499,455994.7011],[138113.8579,455985.4901],[138115.861,455973.8751],[138116.662,455956.6541],[138111.855,455909.7951],[138100.641,455852.9231],[138045.7379,455856.6061],[138037.4719,455814.5231],[138023.745,455749.9941],[138008.9259,455712.7471],[137982.4929,455663.4851],[137950.4929,455608.4291],[137913.4149,455554.4281],[137868.923,455497.0201],[137867.829,455478.6551],[137868.801,455432.4371],[137870.139,455360.5561],[137869.53,455252.6741],[137869.0441,455224.9431],[137939.6179,455169.8771],[137960.3379,455149.4141],[137974.8269,455133.5131],[137964.2039,455130.3201],[137926.4889,455126.2711],[137917.9069,455127.7661],[137910.8959,455127.5451],[137871.2749,455126.2951],[137861.403,455128.6071],[137852.58,455137.854],[137827.794,455169.376],[137811.199,455186.398],[137791.874,455195.224],[137768.558,455202.579],[137713.1029,455217.5001],[137684.7139,455224.7261],[137677.6039,455226.5361],[137641.2639,455235.9931],[137605.9749,455240.6161],[137581.9239,455239.6711],[137516.5959,455228.7431],[137498.8459,455216.2391],[137477.5079,455201.3561],[137469.0189,455195.4351],[137448.6429,455190.1811],[137440.0309,455185.1371],[137429.1079,455176.3111],[137405.301,455149.627],[137400.541,455202.37],[137397.282,455236.207],[137396.58,455257.706],[137396.679,455270.214],[137397.021,455278.3401],[137397.364,455286.4671],[137399.517,455310.3311],[137405.761,455350.1501],[137410.827,455375.6551],[137422.067,455416.867],[137436.855,455456.942],[137499.8989,455605.1721],[137519.2839,455648.1201],[137537.7349,455681.7161],[137554.6339,455706.3151],[137573.6109,455729.3471],[137596.1379,455753.5951],[137620.1129,455777.5591],[137655.2249,455812.3761],[137660.8649,455817.8901],[137749.0999,455909.6401],[137770.5749,455934.3461],[137814.5629,455991.9421],[137853.3629,456053.1531],[137886.6799,456117.5131],[137914.261,456184.5311],[137944.579,456265.4921],[137955.6589,456253.2261]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Oudwijk\"}},{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3825\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[137397.021,455278.3401],[137396.679,455270.214],[137396.58,455257.706],[137397.282,455236.207],[137400.541,455202.37],[137405.301,455149.627],[137429.1079,455176.3111],[137440.0309,455185.1371],[137448.6429,455190.1811],[137469.0189,455195.4351],[137477.5079,455201.3561],[137498.8459,455216.2391],[137516.5959,455228.7431],[137581.9239,455239.6711],[137605.9749,455240.6161],[137641.2639,455235.9931],[137677.6039,455226.5361],[137684.7139,455224.7261],[137713.1029,455217.5001],[137768.558,455202.579],[137791.874,455195.224],[137811.199,455186.398],[137827.794,455169.376],[137852.58,455137.854],[137861.403,455128.6071],[137871.2749,455126.2951],[137910.8959,455127.5451],[137917.9069,455127.7661],[137926.4889,455126.2711],[137964.2039,455130.3201],[137974.8269,455133.5131],[137987.9339,455119.1301],[138021.4849,455075.3441],[138048.2289,455032.5321],[138095.8799,454940.2661],[137935.9419,454963.1951],[137842.0469,454977.2751],[137743.5309,454998.7101],[137599.7479,455034.7511],[137505.6429,455009.7431],[137473.6099,454999.5511],[137463.5349,454995.5571],[137450.9479,454989.7271],[137433.5989,454980.0611],[137370.6069,454942.1001],[137337.8379,454920.8751],[137319.3529,454910.7871],[137313.6819,454902.5921],[137301.7089,454895.6571],[137246.2539,454876.5331],[137224.3029,454868.8621],[137210.4399,454882.3121],[137190.0639,454882.9421],[137169.059,454872.225],[137159.711,454844.8],[137135.345,454768.5161],[137116.8599,454768.5161],[137107.366,454756.768],[137087.664,454770.409],[137058.466,454788.692],[137042.922,454809.707],[137040.9169,454820.0721],[137051.7469,454819.1441],[137058.4939,454821.0721],[137064.2759,454825.8901],[137085.4779,454861.5481],[137090.296,454873.113],[137112.462,454935.755],[137121.1359,454956.7161],[137128.9659,454973.8211],[137154.7459,455022.9711],[137181.9709,455070.9171],[137194.2579,455091.6371],[137205.0999,455107.5381],[137214.9779,455121.2711],[137227.9889,455137.6541],[137247.9789,455160.5651],[137259.7909,455173.0711],[137268.9469,455181.9861],[137280.5109,455192.1051],[137296.8949,455205.2351],[137317.1329,455219.6901],[137335.4439,455232.0991],[137345.4419,455239.5671],[137352.6699,455245.7111],[137353.8839,455246.9001],[137355.0789,455248.2411],[137356.4949,455250.1001],[137357.6679,455252.0201],[137358.574,455254.2061],[137359.16,455256.179],[137359.533,455258.472],[137359.657,455260.649],[137359.533,455262.365],[137359.213,455264.071],[137358.787,455265.618],[137357.1319,455270.0021],[137363.7739,455273.1161],[137372.1649,455276.1481],[137380.3229,455278.0131],[137388.9469,455278.4801],[137397.021,455278.3401]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Abstede, Tolsteegsingel e.o.\"}},{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3827\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[137743.5309,454998.7101],[137842.0469,454977.2751],[137935.9419,454963.1951],[138095.8799,454940.2661],[138122.254,454852.8631],[138139.562,454670.6371],[138141.961,454643.8691],[138165.2209,454645.0281],[138173.4949,454644.6341],[138182.5569,454643.0571],[138192.7999,454641.0861],[138206.1259,454635.514],[138232.8899,454623.507],[138256.7519,454612.984],[138221.1669,454532.8591],[138138.5329,454346.7941],[138047.871,454395.8551],[138006.6619,454406.5741],[137969.574,454407.399],[137929.189,454419.767],[137864.0779,454432.135],[137817.924,454450.275],[137752.814,454484.082],[137708.102,454498.305],[137654.9419,454508.6121],[137603.8429,454504.9021],[137574.9959,454499.1301],[137527.083,454504.0721],[137521.713,454525.8751],[137513.998,454561.5551],[137510.58,454577.3611],[137460.935,454820.5741],[137442.433,454938.7271],[137433.5989,454980.0611],[137450.9479,454989.7271],[137463.5349,454995.5571],[137473.6099,454999.5511],[137505.6429,455009.7431],[137599.7479,455034.7511],[137743.5309,454998.7101]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Rubenslaan en omgeving\"}},{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3831\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[138628.0939,455560.3911],[138648.378,455504.8781],[138658.109,455448.9321],[138657.976,455392.1501],[138654.199,455363.9861],[138639.374,455309.178],[138615.2559,455257.7771],[138593.8569,455225.1801],[138500.1569,455090.5361],[138439.8919,455000.0011],[138419.8079,454965.6171],[138371.0689,454869.4101],[138256.7519,454612.984],[138232.8899,454623.507],[138206.1259,454635.514],[138192.7999,454641.0861],[138182.5569,454643.0571],[138173.4949,454644.6341],[138165.2209,454645.0281],[138141.961,454643.8691],[138139.562,454670.6371],[138122.254,454852.8631],[138095.8799,454940.2661],[138048.2289,455032.5321],[138021.4849,455075.3441],[137987.9339,455119.1301],[137974.8269,455133.5131],[138014.9969,455145.5891],[138075.5679,455176.1261],[138168.2119,455283.3641],[138262.8829,455406.4671],[138339.9079,455443.4451],[138388.4359,455470.7611],[138602.9239,455610.0971],[138628.0939,455560.3911]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Schildersbuurt\"}},{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3832\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[138440.656,456032.8551],[138471.319,455915.0601],[138494.741,455833.6291],[138503.993,455807.0631],[138513.245,455782.9131],[138528.5309,455749.9081],[138542.6099,455721.3301],[138559.1549,455691.9411],[138602.9239,455610.0971],[138388.4359,455470.7611],[138339.9079,455443.4451],[138262.8829,455406.4671],[138168.2119,455283.3641],[138075.5679,455176.1261],[138014.9969,455145.5891],[137974.8269,455133.5131],[137960.3379,455149.4141],[137939.6179,455169.8771],[137869.0441,455224.9431],[137869.53,455252.6741],[137870.139,455360.5561],[137868.801,455432.4371],[137867.829,455478.6551],[137868.923,455497.0201],[137913.4149,455554.4281],[137950.4929,455608.4291],[137982.4929,455663.4851],[138008.9259,455712.7471],[138023.745,455749.9941],[138037.4719,455814.5231],[138045.7379,455856.6061],[138100.641,455852.9231],[138111.855,455909.7951],[138116.662,455956.6541],[138115.861,455973.8751],[138113.8579,455985.4901],[138107.4499,455994.7011],[138097.0369,456005.9151],[138090.2279,456009.9201],[138062.149,456025.7861],[138050.965,456079.3021],[138040.024,456123.8171],[138033.5809,456142.4261],[138022.8839,456158.7241],[137974.6229,456225.9821],[137955.6589,456253.2261],[137944.579,456265.4921],[137994.7159,456404.6501],[138036.4749,456404.1881],[138063.2109,456402.7021],[138137.4779,456398.9891],[138151.5889,456398.9891],[138162.7289,456400.4741],[138176.0969,456404.9301],[138197.0769,456413.1141],[138280.8139,456445.7771],[138329.665,456461.836],[138440.656,456032.8551]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Wilhelminapark en omgeving\"}},{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3836\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[136933.851,456342.5271],[136933.806,456332.9211],[136933.075,456322.8591],[136929.429,456299.2131],[136927.194,456285.4251],[136914.901,456200.4611],[136911.135,456180.7681],[136905.558,456145.6401],[136903.697,456125.0911],[136902.755,456107.2031],[136899.312,456079.9641],[136937.0969,456060.1971],[136945.5079,456055.4401],[136957.7579,456047.7561],[136961.9629,456045.0121],[136965.8019,456042.9991],[137003.4659,456028.911],[137037.1979,456023.8341],[137045.426,456021.821],[137054.567,456018.345],[137060.0519,456014.8691],[137067.3659,456009.0141],[137075.4099,456000.9641],[137082.7239,455992.0451],[137090.9509,455980.3351],[137101.0069,455964.0521],[137107.5889,455951.7941],[137125.8979,455912.2291],[137114.9019,455897.0421],[137105.3949,455884.6011],[137097.5329,455875.4531],[137079.0669,455855.1451],[137070.2909,455847.0951],[137059.5499,455839.1231],[136964.4139,455789.0241],[136952.3809,455785.3681],[136953.477,455766.7181],[136955.203,455752.3861],[136957.791,455737.5371],[136960.724,455724.0691],[136964.348,455710.6011],[136967.971,455698.2551],[136973.493,455681.6791],[136978.497,455667.5201],[136980.9639,455662.0601],[136970.8219,455658.6791],[136927.1669,455643.8611],[136888.7189,455632.6461],[136859.0819,455624.6361],[136804.6129,455606.2131],[136786.404,455603.7851],[136784.852,455609.679],[136781.056,455625.91],[136778.122,455639.766],[136771.22,455676.004],[136767.837,455692.919],[136766.734,455697.242],[136763.326,455709.07],[136759.703,455720.121],[136755.389,455731.344],[136751.075,455741.186],[136748.248,455747.158],[136743.656,455756.899],[136738.652,455766.655],[136733.13,455777.878],[136727.954,455789.792],[136724.675,455798.425],[136722.26,455805.677],[136719.672,455814.483],[136714.582,455832.893],[136711.39,455846.102],[136706.645,455867.945],[136701.037,455894.469],[136699.57,455900.513],[136696.033,455910.959],[136692.54,455918.989],[136687.536,455927.449],[136683.222,455934.356],[136679.081,455940.227],[136673.387,455946.443],[136665.882,455953.177],[136659.67,455957.839],[136651.56,455963.019],[136644.1409,455966.645],[136637.756,455969.019],[136633.443,455970.228],[136628.2659,455970.9181],[136622.0539,455971.2641],[136615.6699,455971.2641],[136607.3879,455970.5731],[136599.6229,455969.5371],[136585.6469,455967.6381],[136574.3889,455966.6021],[136557.9969,455966.6021],[136538.6719,455967.4651],[136528.6639,455968.8461],[136518.6559,455970.918],[136508.476,455973.854],[136502.436,455976.271],[136496.397,455979.897],[136490.876,455983.523],[136481.731,455991.12],[136476.382,455996.991],[136474.138,455999.754],[136471.895,456004.416],[136470.342,456009.596],[136469.307,456015.639],[136468.962,456022.546],[136469.134,456026.862],[136469.825,456031.352],[136473.448,456048.014],[136475.691,456057.166],[136477.589,456068.044],[136478.97,456079.872],[136479.832,456094.721],[136480.005,456102.836],[136479.832,456110.088],[136478.97,456118.549],[136476.382,456137.024],[136473.448,456151.788],[136469.37,456163.7461],[136492.1279,456168.0771],[136580.2529,456176.4931],[136608.4099,456220.7681],[136642.7819,456259.9211],[136689.5869,456310.4181],[136697.2659,456317.7361],[136704.5799,456323.9571],[136714.0869,456331.2751],[136723.5939,456337.4951],[136734.5639,456342.6181],[136747.2339,456346.9821],[136762.5369,456350.3021],[136850.6629,456354.6021],[136908.4379,456355.3341],[136933.3599,456359.1251],[136933.851,456342.5271]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Domplein, Neude, Janskerkhof\"}},{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3841\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[137012.8349,456347.2841],[137079.7509,456334.1111],[137117.416,456339.2041],[137119.134,456325.086],[137121.899,456308.491],[137124.112,456298.673],[137125.633,456293.832],[137127.984,456288.024],[137131.856,456280.833],[137134.345,456276.961],[137137.941,456272.398],[137141.813,456268.11],[137147.622,456262.303],[137155.09,456255.665],[137165.116,456248.058],[137183.785,456235.198],[137226.184,456207.386],[137233.653,456202.082],[137240.069,456196.891],[137245.338,456191.722],[137248.831,456187.265],[137251.361,456182.808],[137252.807,456177.387],[137253.048,456172.448],[137252.084,456167.388],[137250.277,456162.4491],[137247.9879,456157.8721],[137244.9159,456153.0541],[137204.0589,456097.5831],[137183.9599,456070.2951],[137180.5269,456064.9951],[137178.659,456061.139],[137177.154,456057.044],[137176.371,456054.032],[137175.286,456047.406],[137175.286,456043.551],[137175.527,456039.456],[137176.552,456031.024],[137194.485,455938.3031],[137156.7039,455929.8611],[137125.8979,455912.2291],[137107.5889,455951.7941],[137101.0069,455964.0521],[137090.9509,455980.3351],[137082.7239,455992.0451],[137075.4099,456000.9641],[137067.3659,456009.0141],[137060.0519,456014.8691],[137054.567,456018.345],[137045.426,456021.821],[137037.1979,456023.8341],[137003.4659,456028.911],[136965.8019,456042.9991],[136961.9629,456045.0121],[136957.7579,456047.7561],[136945.5079,456055.4401],[136937.0969,456060.1971],[136899.312,456079.9641],[136902.755,456107.2031],[136903.697,456125.0911],[136905.558,456145.6401],[136911.135,456180.7681],[136914.901,456200.4611],[136927.194,456285.4251],[136929.429,456299.2131],[136933.075,456322.8591],[136933.806,456332.9211],[136933.851,456342.5271],[136933.3599,456359.1251],[136941.1639,456359.3591],[137012.8349,456347.2841]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Nobelstraat en omgeving\"}},{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3843\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[136982.9829,455657.5921],[136993.0029,455641.2901],[137029.5289,455580.4531],[137035.0509,455571.1291],[137054.1519,455541.3801],[137082.8609,455496.7591],[137086.0389,455490.3631],[137118.5579,455416.4931],[137160.8919,455322.0131],[137187.7579,455260.0211],[137194.057,455243.2731],[137199.406,455228.0781],[137210.795,455192.3351],[137212.7609,455189.5771],[137227.7909,455178.6951],[137235.5549,455172.4791],[137241.2499,455167.2991],[137247.9789,455160.5651],[137227.9889,455137.6541],[137214.9779,455121.2711],[137205.0999,455107.5381],[137194.2579,455091.6371],[137181.9709,455070.9171],[137154.7459,455022.9711],[137128.9659,454973.8211],[137121.1359,454956.7161],[137112.462,454935.755],[137090.296,454873.113],[137085.4779,454861.5481],[137064.2759,454825.8901],[137058.4939,454821.0721],[137051.7469,454819.1441],[137040.9169,454820.0721],[136984.2869,454824.9271],[136970.4689,454820.6751],[136955.5879,454866.3891],[136924.53,454866.1301],[136891.3999,454901.1811],[136884.6709,454908.7791],[136880.5299,454914.7791],[136878.4589,454918.6211],[136876.0429,454923.2831],[136873.455,454929.6281],[136871.557,454936.4051],[136870.349,454943.3121],[136869.659,454953.6291],[136869.659,454962.3921],[136870.177,454971.1981],[136871.212,454982.0761],[136873.283,454997.9611],[136876.216,455015.8331],[136881.738,455041.2151],[136891.745,455085.2451],[136892.694,455090.6411],[136892.349,455102.0371],[136884.412,455160.0531],[136878.2,455190.7881],[136868.414,455228.6741],[136865.087,455239.6531],[136858.53,455267.2801],[136848.522,455313.2091],[136835.779,455374.3181],[136832.408,455390.6511],[136826.2,455426.1761],[136813.668,455485.3581],[136803.832,455530.0361],[136787.785,455598.5421],[136786.404,455603.7851],[136804.6129,455606.2131],[136859.0819,455624.6361],[136888.7189,455632.6461],[136927.1669,455643.8611],[136970.8219,455658.6791],[136980.9639,455662.0601],[136982.9829,455657.5921]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Lange Nieuwstraat en omgeving\"}},{\"type\":\"Feature\",\"id\":\"cbs_buurten_2020.3844\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[137198.476,455917.667],[137208.113,455861.29],[137211.486,455844.906],[137214.499,455832.497],[137217.149,455823.101],[137219.678,455815.995],[137222.45,455810.452],[137225.942,455805.394],[137229.557,455801.297],[137234.134,455797.202],[137243.289,455790.456],[137259.191,455780.337],[137276.538,455769.736],[137292.921,455759.135],[137300.812,455753.713],[137307.859,455748.534],[137314.184,455743.594],[137320.086,455738.536],[137322.796,455735.764],[137325.627,455732.031],[137330.144,455725.404],[137332.192,455721.79],[137334.06,455717.334],[137335.565,455712.876],[137336.288,455704.805],[137335.565,455696.011],[137334.541,455691.554],[137332.433,455685.5311],[137329.7229,455678.7851],[137327.1329,455673.6051],[137319.0609,455660.7151],[137314.2429,455653.9691],[137292.4979,455628.5501],[137290.2099,455625.7801],[137280.9339,455612.1671],[137277.0789,455605.6621],[137273.104,455597.471],[137269.911,455589.159],[137268.224,455583.3771],[137267.14,455577.956],[137266.658,455573.137],[137266.658,455568.318],[137267.381,455561.091],[137268.405,455555.789],[137290.571,455457.972],[137302.617,455396.534],[137308.881,455365.935],[137311.772,455355.816],[137314.663,455347.625],[137318.518,455338.71],[137324.782,455326.423],[137346.466,455288.717],[137352.61,455277.634],[137357.1319,455270.0021],[137358.787,455265.618],[137359.213,455264.071],[137359.533,455262.365],[137359.657,455260.649],[137359.533,455258.472],[137359.16,455256.179],[137358.574,455254.2061],[137357.6679,455252.0201],[137356.4949,455250.1001],[137355.0789,455248.2411],[137353.8839,455246.9001],[137352.6699,455245.7111],[137345.4419,455239.5671],[137335.4439,455232.0991],[137317.1329,455219.6901],[137296.8949,455205.2351],[137280.5109,455192.1051],[137268.9469,455181.9861],[137259.7909,455173.0711],[137247.9789,455160.5651],[137241.2499,455167.2991],[137235.5549,455172.4791],[137227.7909,455178.6951],[137212.7609,455189.5771],[137210.795,455192.3351],[137199.406,455228.0781],[137194.057,455243.2731],[137187.7579,455260.0211],[137160.8919,455322.0131],[137118.5579,455416.4931],[137086.0389,455490.3631],[137082.8609,455496.7591],[137054.1519,455541.3801],[137035.0509,455571.1291],[137029.5289,455580.4531],[136993.0029,455641.2901],[136982.9829,455657.5921],[136980.9639,455662.0601],[136978.497,455667.5201],[136973.493,455681.6791],[136967.971,455698.2551],[136964.348,455710.6011],[136960.724,455724.0691],[136957.791,455737.5371],[136955.203,455752.3861],[136953.477,455766.7181],[136952.3809,455785.3681],[136964.4139,455789.0241],[137059.5499,455839.1231],[137070.2909,455847.0951],[137079.0669,455855.1451],[137097.5329,455875.4531],[137105.3949,455884.6011],[137114.9019,455897.0421],[137125.8979,455912.2291],[137156.7039,455929.8611],[137194.485,455938.3031],[137198.476,455917.667]]]]},\"geometry_name\":\"geom\",\"properties\":{\"buurtnaam\":\"Nieuwegracht-Oost\"}}],\"crs\":{\"type\":\"name\",\"properties\":{\"name\":\"urn:ogc:def:crs:EPSG::28992\"}}}";

    [Test]
    public void TestGetPropertiesAmsterdam()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Amsterdam;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringAmsterdam);

        geojson.GotoNextFeature();

        double diameter1 = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
        double bobBeginPunt1 = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
        double bobEindPunt1 = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);

        Assert.AreEqual(297, diameter1);
        Assert.AreEqual(-1, bobBeginPunt1);
        Assert.AreEqual(-1.6, bobEindPunt1, 0.1);

        geojson.GotoNextFeature();

        double diameter2 = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
        double bobBeginPunt2 = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
        double bobEindPunt2 = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);

        Assert.AreEqual(234, diameter2);
        Assert.AreEqual(-1, bobBeginPunt2);
        Assert.AreEqual(-1.1, bobEindPunt2, 0.1);
    }

    [Test]
    public void TestGetPropertiesPdok()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringPdok);

        geojson.GotoNextFeature();

        double diameter1 = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
        double bobBeginPunt1 = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
        double bobEindPunt1 = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);

        Assert.AreEqual(6000, diameter1);
        Assert.AreEqual(0.85, bobBeginPunt1, 0.1);
        Assert.AreEqual(0.85, bobEindPunt1, 0.1);

        geojson.GotoNextFeature();

        double diameter2 = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
        double bobBeginPunt2 = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
        double bobEindPunt2 = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);

        Assert.AreEqual(150, diameter2);
        Assert.AreEqual(-1.25, bobBeginPunt2);
        Assert.AreEqual(-1.25, bobEindPunt2);
    }

    [Test]
    public void TestCoordinatesAmsterdam()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Amsterdam;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringAmsterdam);

        geojson.GotoNextFeature();

        var geom1 = geojson.getGeometryLineString();
        Assert.AreEqual(4, geom1.Count);
        Assert.AreEqual(geom1[0], 4.90698478562009, 0.000001);
        Assert.AreEqual(geom1[1], 52.3429529014738, 0.000001);
        Assert.AreEqual(geom1[2], 4.90702608778446, 0.000001);
        Assert.AreEqual(geom1[3], 52.3428382055533, 0.000001);

        geojson.GotoNextFeature();
        var geom2 = geojson.getGeometryLineString();
        Assert.AreEqual(4, geom2.Count);
        Assert.AreEqual(geom2[0], 4.90698478562009, 0.000001);
        Assert.AreEqual(geom2[1], 52.3429529014738, 0.000001);
        Assert.AreEqual(geom2[2], 4.90665251262403, 0.000001);
        Assert.AreEqual(geom2[3], 52.3429023897671, 0.000001);
    }

    [Test]
    public void TestCoordinatesPdok()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringPdok);

        geojson.GotoNextFeature();

        var geom1 = geojson.getGeometryLineString();
        Assert.AreEqual(4, geom1.Count);
        Assert.AreEqual(geom1[0], 132803.42, 0.1);
        Assert.AreEqual(geom1[1], 457145.83, 0.1);
        Assert.AreEqual(geom1[2], 132798.02, 0.1);
        Assert.AreEqual(geom1[3], 457153.57, 0.1);

        geojson.GotoNextFeature();
        var geom2 = geojson.getGeometryLineString();        
        Assert.AreEqual(4, geom2.Count);
        Assert.AreEqual(geom2[0], 132652.29, 0.1);
        Assert.AreEqual(geom2[1], 457103.23, 0.1);
        Assert.AreEqual(geom2[2], 132655.03, 0.1);
        Assert.AreEqual(geom2[3], 457110.39, 0.1);

    }

    [Test]
    public void TestCoordinatesMultipleSegmentsPdok()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringMultipleSegmentPdok);

        geojson.GotoNextFeature();

        var geom1 = geojson.getGeometryLineString();
       // Assert.AreEqual(4, geom1.Count);
        Assert.AreEqual(geom1[0], 128714.08, 0.1);
        Assert.AreEqual(geom1[1], 456715.91, 0.1);
        Assert.AreEqual(geom1[2], 128726.92, 0.1);
        Assert.AreEqual(geom1[3], 456784.05, 0.1);

        geojson.GotoNextFeature();
        var geom2 = geojson.getGeometryLineString();
        //Assert.AreEqual(4, geom2.Count);
        Assert.AreEqual(geom2[0], 129029.42, 0.1);
        Assert.AreEqual(geom2[1], 456653.39, 0.1);
        Assert.AreEqual(geom2[2], 129029.21, 0.1);
        Assert.AreEqual(geom2[3], 456654.99, 0.1);

        geojson.GotoNextFeature();
        var geom3 = geojson.getGeometryLineString();
        //Assert.AreEqual(4, geom2.Count);
        Assert.AreEqual(geom3[0], 128995.88, 0.1);
        Assert.AreEqual(geom3[1], 456709.91, 0.1);
        Assert.AreEqual(geom3[2], 128980.46, 0.1);
        Assert.AreEqual(geom3[3], 456704.58, 0.1);

    }

    [Test]
    public void TestReadingMixedValuesAmsterdam()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = SewerageApiType.Amsterdam;

        GeoJSON geojson = new GeoJSON(testStringAmsterdamCablesAndPipelineApi);
        geojson.GotoNextFeature();

        var stringValue = geojson.getPropertyStringValue("id");
        var floatValueWithNull = geojson.getPropertyFloatValue("diam_mm");

        var floatWrappedInQuotesAsFloat = geojson.getPropertyFloatValue("diepte");
        var floatWrappedInQuotesAsString = geojson.getPropertyStringValue("diepte");

        Assert.AreEqual("ligging_lijn_totaal.2302136", stringValue);
        Assert.AreEqual(0, floatValueWithNull);

        Assert.AreEqual(-1, floatWrappedInQuotesAsFloat);
        Assert.AreEqual("-1", floatWrappedInQuotesAsString);
    }
    [Test]
    public void TestReadingPropertyStringValueUtrecht()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = SewerageApiType.Pdok;

        GeoJSON geojson = new GeoJSON(testStringMultipleSegmentPdok);
        geojson.GotoNextFeature();

        var stringValue = geojson.getPropertyStringValue("Dataset");
        Assert.AreEqual("Utrecht", stringValue);
    }

    [Test]
    public void TestConvertCoordinatesAmsterdam()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Amsterdam;
        Config.activeConfiguration.RelativeCenterRD.x = 10;
        Config.activeConfiguration.RelativeCenterRD.y = 20;

        SewerageLayer seweragelayer = new SewerageLayer();

        var unityPoint1 = seweragelayer.GetUnityPoint(4.90698478562009, 52.3429529014738, 0.3);
        Assert.AreEqual(122265.11, unityPoint1.x, 0.01);
        Assert.AreEqual(483981.25, unityPoint1.z, 0.01);
    }

    [Test]
    public void TestConvertCoordinatesPdok()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;
        Config.activeConfiguration.RelativeCenterRD.x = 10;
        Config.activeConfiguration.RelativeCenterRD.y = 20;
        
        SewerageLayer seweragelayer = new SewerageLayer();

        var unityPoint1 = seweragelayer.GetUnityPoint(132803.42, 457145.83, 0.3);
        Assert.AreEqual(132793.42, unityPoint1.x, 0.1);
        Assert.AreEqual(457125.83, unityPoint1.z, 0.1);
    }

    [Test]
    public void TestManholesPropertiesAmsterdam()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Amsterdam;
        SewerageLayer seweragelayer = new SewerageLayer();
        
        GeoJSON geojson = new GeoJSON(testStringManholesAmsterdam);
        
        geojson.GotoNextFeature();
        var putdekselhoogte1 =  geojson.getPropertyFloatValue(seweragelayer.PutdekselhoogteString);
        var point1 = geojson.getGeometryPoint2DDouble();

        Assert.AreEqual(0.20, putdekselhoogte1, 0.001);
        Assert.AreEqual(2, point1.Length);
        Assert.AreEqual(4.91561158208928, point1[0], 0.001);
        Assert.AreEqual(52.342962335247, point1[1], 0.001);

        geojson.GotoNextFeature();
        var putdekselhoogte2 = geojson.getPropertyFloatValue(seweragelayer.PutdekselhoogteString);
        var point2 = geojson.getGeometryPoint2DDouble();

        Assert.AreEqual(0.68, putdekselhoogte2, 0.001);
        Assert.AreEqual(2, point2.Length);
        Assert.AreEqual(4.90876406244186, point2[0], 0.001);
        Assert.AreEqual(52.3425051475584, point2[1], 0.001);
        
    }


    [Test]
    public void TestManholesPropertiesPdok()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;
        SewerageLayer seweragelayer = new SewerageLayer();

        GeoJSON geojson = new GeoJSON(testStringManholesPdok);

        geojson.GotoNextFeature();
        var putdekselhoogte1 = geojson.getPropertyFloatValue(seweragelayer.PutdekselhoogteString);
        var point1 = geojson.getGeometryPoint2DDouble();
        Assert.AreEqual(1.530, putdekselhoogte1, 0.001);
        Assert.AreEqual(2, point1.Length);
        Assert.AreEqual(132303.35, point1[0], 0.001);
        Assert.AreEqual(457990.35, point1[1], 0.001);

        geojson.GotoNextFeature();
        var putdekselhoogte2 = geojson.getPropertyFloatValue(seweragelayer.PutdekselhoogteString);
        var point2 = geojson.getGeometryPoint2DDouble();
        Assert.AreEqual(1.540, putdekselhoogte2, 0.001);
        Assert.AreEqual(2, point2.Length);
        Assert.AreEqual(132347.73, point2[0], 0.001);
        Assert.AreEqual(457958.06, point2[1], 0.001);

    }

    [Test]
    public void TestApiAmsterdam()
    {

    }

    [Test]
    public void TestApiPdokEmpty()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringPdokEmpty);

        int counter = 0;
        while (geojson.GotoNextFeature())
        {
            counter++;
        }

        Assert.AreEqual(0, counter);
        
    }

    [Test]
    public void TestApiPdokOne()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringPdokOne);

        geojson.GotoNextFeature();

        double diameter = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
        double bobBeginPunt = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
        double bobEindPunt = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);
        var geom = geojson.getGeometryLineString();

        Assert.AreEqual(800, diameter);
        Assert.AreEqual(0.06, bobBeginPunt, 0.1);
        Assert.AreEqual(0.05, bobEindPunt, 0.1);
        Assert.AreEqual(4, geom.Count);
    }

    [Test]
    public void TestApiAmsterdamOne()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Amsterdam;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringAmsterdamOne);

        geojson.GotoNextFeature();

        double diameter = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
        double bobBeginPunt = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
        double bobEindPunt = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);
        var geom = geojson.getGeometryLineString();

        Assert.AreEqual(297, diameter);
        Assert.AreEqual(-1, bobBeginPunt);
        Assert.AreEqual(-1.60, bobEindPunt, 0.1);
        Assert.AreEqual(4, geom.Count);
    }

    [Test]
    public void TestApiAmsterdamOneWhile()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Amsterdam;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringAmsterdamOne);

        while (geojson.GotoNextFeature())
        {
            double diameter = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
            double bobBeginPunt = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
            double bobEindPunt = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);
            var geom = geojson.getGeometryLineString();

            Assert.AreEqual(297, diameter);
            Assert.AreEqual(-1, bobBeginPunt);
            Assert.AreEqual(-1.60, bobEindPunt, 0.1);
            Assert.AreEqual(4, geom.Count);
        }
    }

    [Test]
    public void TestApiPdokOneWhile()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;

        SewerageLayer seweragelayer = new SewerageLayer();
        GeoJSON geojson = new GeoJSON(testStringPdokOne);

        while (geojson.GotoNextFeature())
        {
            double diameter = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
            double bobBeginPunt = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
            double bobEindPunt = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);
            var geom = geojson.getGeometryLineString();

            Assert.AreEqual(800, diameter);
            Assert.AreEqual(0.06, bobBeginPunt, 0.001);
            Assert.AreEqual(0.05, bobEindPunt, 0.001);

            Assert.AreEqual(4, geom.Count);
        }
        

    }

    [Test]
    public void TestBuurtnamen()
    {
        //https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?service=WFS&version=2.0.0&request=GetFeature&TypeNames=kadastralekaartv4:openbareruimtenaam&&propertyName=plaatsingspunt,tekst,hoek,relatieveHoogteligging,openbareRuimteType&outputformat=geojson&srs=EPSG:28992&bbox=

        Config.activeConfiguration = new ConfigurationFile();
        GeoJSON customJsonHandler = new GeoJSON(testStringBuurtnamen);

        customJsonHandler.GotoNextFeature();
        var coordinates = customJsonHandler.getGeometryMultiPolygonString();

        Assert.AreEqual(true, coordinates.Any());

        var coord1 = coordinates[0];
        Assert.AreEqual(137944.579, coord1, 0.1);

    }

    //[UnityTest]
    public IEnumerator TestApiPdok()
    {
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;
        Config.activeConfiguration.sewerPipesWfsUrl = "https://geodata.nationaalgeoregister.nl/rioned/gwsw/wfs/v1_0?SERVICE=WFS&language=eng&SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=gwsw:beheer_leiding&SRSNAME=urn:ogc:def:crs:EPSG::28992&outputFormat=application/json&BBOX=";
        Config.activeConfiguration.sewerManholesWfsUrl = "https://geodata.nationaalgeoregister.nl/rioned/gwsw/wfs/v1_0?SERVICE=WFS&language=eng&SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=gwsw:beheer_put&SRSNAME=urn:ogc:def:crs:EPSG::28992&outputFormat=application/json&BBOX=";

        SewerageLayer seweragelayer = new SewerageLayer();

        int Xmin = 123000;
        int Ymin = 443000;
        int Xmax = 145000;
        int Ymax = 463000;

        string url;

        int counter = 0;

        for (int x = Xmin; x < Xmax; x += 1000)
        {
            for (int y = Ymin; y < Ymax; y += 1000)
            {
                counter++;

                url = Config.activeConfiguration.sewerPipesWfsUrl + $"{x},{y},{x + 1000},{y + 1000}";

                var sewerageRequest = UnityWebRequest.Get(url);

                yield return sewerageRequest.SendWebRequest();

                if (!sewerageRequest.isNetworkError && !sewerageRequest.isHttpError)
                {
                    try
                    {
                        GeoJSON geojson = new GeoJSON(sewerageRequest.downloadHandler.text);
                        while (geojson.GotoNextFeature())
                        {
                            double diameter = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
                            double bobBeginPunt = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
                            double bobEindPunt = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);
                            //var coordinates = geojson.getGeometryLineString();
                            //Assert.AreEqual(4, coordinates.Count);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"url:{url} exception:{e.Message}");
                    }
                }

                yield return null;




            }

        }



    }


    // [Test]
    public void TestFilePdok()
    {

        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.sewerageApiType = Amsterdam3D.Sewerage.SewerageApiType.Pdok;
        SewerageLayer seweragelayer = new SewerageLayer();

        //var jsontext = System.IO.File.ReadAllText(@"F:\Data\sewerage\data_utrecht_leidingen.json");
        //var jsontext = System.IO.File.ReadAllText(@"F:\Data\sewerage\data_utrecht_leidingen_128000_456000_129000_457000.json");
        var jsontext = System.IO.File.ReadAllText(@"F:\Data\sewerage\data_utrecht_leidingen_count_error.json");

        GeoJSON geojson = new GeoJSON(jsontext);

        while (geojson.GotoNextFeature())
        {
            double diameter = geojson.getPropertyFloatValue(seweragelayer.DiameterString);
            double bobBeginPunt = geojson.getPropertyFloatValue(seweragelayer.BobBeginPuntString);
            double bobEindPunt = geojson.getPropertyFloatValue(seweragelayer.BobEindPuntString);

            var geom = geojson.getGeometryLineString();

            Assert.AreEqual(4, geom.Count);

           // Debug.Log($"diameter:{diameter} bobBeginPunt:{bobBeginPunt} bobEindPunt:{bobEindPunt}");

        }



    }
}