import 'whatwg-fetch';
import { nlmaps } from '../nlmaps/dist/nlmaps.es.js';
import { pointQueryChain } from './lib.js';
import 'leaflet.markercluster';
import emitonoff from 'emitonoff';
const mora = {};
emitonoff(mora);

mora.createMap = async function(config) {
  //create map
  let map = nlmaps.createMap(config);
  //subscribe chain of API calls to the nlmaps click event
  nlmaps.on("mapclick", async function(click) {
    const result = await pointQueryChain(click);
    mora.emit("query-results", result);
  });

  mora.on("query-results", function(res) {
    if (res.dichtstbijzijnd_adres !== null) {
      let adres = res.dichtstbijzijnd_adres;
      let display_string = `${adres.openbare_ruimte} ${adres.huisnummer}
${adres.huisletter ? adres.huisletter : ""}
${adres.huisnummer_toevoeging ? "-" + adres.huisnummer_toevoeging : ""}
, ${adres.postcode} ${adres.woonplaats}`;
      document.getElementById(
        "nlmaps-geocoder-control-input"
      ).value = display_string;
    } else {
      document.getElementById("nlmaps-geocoder-control-input").value = "";
    }
  });

  nlmaps.on("search-select", async function(e) {
    let point = {
      latlng: { lat: e.latlng.coordinates[1], lng: e.latlng.coordinates[0] },
      resultObject: e.resultObject
    };
    const result = await pointQueryChain(point);
    mora.emit("query-results", result);
    //emit mapclick so we can place a marker
    nlmaps.emit("mapclick", point);
  });

  //attach user-supplied event handlers
  if (typeof config.clickHandlers === "function") {
    nlmaps.on("mapclick", config.clickHandlers);
  } else if (Array.isArray(config.clickHandlers)) {
    config.clickHandlers.forEach(f => {
      if (typeof f === "function") {
        nlmaps.on("mapclick", f);
      }
    });
  }
  if (typeof config.onQueryResult === "function") {
    mora.on("query-results", config.onQueryResult);
  } else if (Array.isArray(config.onQueryResult)) {
    config.onQueryResult.forEach(f => {
      if (typeof f === "function") {
        mora.on("query-results", f);
      }
    });
  }
  //this is the only private subscription we do here, since it belongs to the map viewport.
  //setup click and feature handlers
  //let clicks = nlmaps.clickProvider(map);
  let singleMarker = nlmaps.singleMarker(map);
  //clicks.subscribe(singleMarker);

  //partially apply singleMarker for search results listener
  function markerFromSearch(click) {
    singleMarker(1, click, false, true);
  }

  nlmaps.on("mapclick", markerFromSearch);
  return map;
};

const pointquery = mora;
export default pointquery;
