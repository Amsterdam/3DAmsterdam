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

                if (sewerageRequest.result != UnityWebRequest.Result.ConnectionError && sewerageRequest.result != UnityWebRequest.Result.ProtocolError)
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