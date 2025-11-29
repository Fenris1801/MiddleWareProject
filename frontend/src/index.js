function transformOpenRouteServiceToItinerary(geoJson, origin, destination) {
    const itineraryData = {
        origin: { lat: origin.lat, lng: origin.lon },
        destination: { lat: destination.lat, lng: destination.lon },
        pickupStation: geoJson.features[0].pickupStation ? { lat: geoJson.features[0].pickupStation.Address.lat, lng: geoJson.features[0].pickupStation.Address.lon } : null,
        dropoffStation: geoJson.features[0].dropoffStation ? { lat: geoJson.features[0].dropoffStation.Address.lat, lng: geoJson.features[0].dropoffStation.Address.lon } : null,
        route: []
    };

    const feat = geoJson.features[0];
    const summary = feat.properties.summary;
    const segments = feat.properties.segments;
    console.log(segments[0].distance);

    const distance = summary.distance;
    const duration = summary.duration;

    const distKm = (distance / 1000).toFixed(2) + " km";
    const timeMin = Math.round(duration / 60) + " min";

    if (geoJson.features && geoJson.features[0]) {
        const feat = geoJson.features[0];
        if (feat.geometry && feat.geometry.type === 'LineString') {
            // Convertir [long, lat] -> [lat, long]
            itineraryData.route = feat.geometry.coordinates.map(c => [c[1], c[0]]);
        }
    }

    const infos = [
        `Distance totale : ${distKm}`,
        `Durée estimée : ${timeMin}`
    ];

    if (segments.length === 3) {
        infos.push(`Distance avant velo : ${(segments[0].distance / 1000).toFixed(2) + " km\n"}`);
        infos.push(`Distance en velo : ${(segments[1].distance / 1000).toFixed(2) + " km\n"}`);
        infos.push(`Distance après velo : ${(segments[2].distance / 1000).toFixed(2) + " km\n"}`);
    }
    else { infos.push(`Plus rapide sans vélo !`) }

    const messagesComponent = document.querySelector('compo-messages');
    messagesComponent.setInfos(infos);

    var steps = [];
    segments.forEach((segment, index) => {
        segment.steps.forEach(step => {
            steps.push(step.instruction);
        });
    });

    messagesComponent.setSteps(steps);



    return itineraryData;
}

const itineraryComponent = document.querySelector('compo-itinerary');
itineraryComponent.addEventListener('request-itinerary', async (event) => {
    const { origin, destination } = event.detail;

    const fromLat = parseFloat(origin.lat);
    const fromLon = parseFloat(origin.lon);
    const toLat = parseFloat(destination.lat);
    const toLon = parseFloat(destination.lon);

    if (
        Number.isNaN(fromLat) || Number.isNaN(fromLon) ||
        Number.isNaN(toLat) || Number.isNaN(toLon)
    ) {
        alert("Merci de choisir les adresses dans la liste de suggestions.");
        return;
    }

    const baseUrl = 'http://localhost:8080/ServiceGPS/itinerary';
    const url = `${baseUrl}?fromLat=${fromLat}&fromLon=${fromLon}&toLat=${toLat}&toLon=${toLon}`;

    try {
        const res = await fetch(url);
        if (!res.ok) {
            throw new Error(`Erreur HTTP ${res.status}`);
        }
        const geoJsonData = JSON.parse(await res.json());
        console.log('Données GeoJSON reçues :', geoJsonData);

        const displayData = transformOpenRouteServiceToItinerary(geoJsonData, {
            lat: fromLat,
            lon: fromLon
        }, {
            lat: toLat,
            lon: toLon
        });

        setItinerary(displayData);
    } catch (err) {
        console.error('Erreur lors de la récupération de l\'itinéraire :', err);
        alert("Impossible de récupérer l'itinéraire.");
    }
});

export let map = null;
let markers = {};
let routePolyline = null;

export function initMap() {
    const container = document.getElementById("map");
    if (!container) return;

    map = L.map(container).setView([43.7, 7.26], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    requestAnimationFrame(() => map.invalidateSize());
}

export function setItinerary(data) {
    if (!map) return;

    Object.values(markers).forEach(m => map.removeLayer(m));
    markers = {};

    if (routePolyline) {
        map.removeLayer(routePolyline);
        routePolyline = null;
    }

    const bounds = [];

    function add(key, point, popup) {
        if (!point) return;
        markers[key] = L.marker([point.lat, point.lng], { title: popup })
            .addTo(map)
            .bindPopup(popup);
        bounds.push([point.lat, point.lng]);
    }

    add("origin", data.origin, "Origine");
    add("destination", data.destination, "Destination");
    add("pickup", data.pickupStation, "Station de départ vélo");
    add("dropoff", data.dropoffStation, "Station d’arrivée vélo");

    if (Array.isArray(data.route) && data.route.length > 1) {
        routePolyline = L.polyline(data.route, { weight: 4 }).addTo(map);
        bounds.push(...data.route);
    }

    if (bounds.length) map.fitBounds(bounds, { padding: [40, 40] });
}

window.addEventListener("DOMContentLoaded", () => {
    initMap();
});
