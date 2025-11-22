const itineraryComponent = document.querySelector('compo-itinerary');
itineraryComponent.addEventListener('request-itinerary', async (event) => {
    const { origin, destination } = event.detail;

    // Sécuriser / parser les coordonnées
    const fromLat = parseFloat(origin.lat);
    const fromLon = parseFloat(origin.lon);
    const toLat = parseFloat(destination.lat);
    const toLon = parseFloat(destination.lon);

    // Important : vérifier qu’on a bien des coords (que l’utilisateur a cliqué sur une suggestion)
    if (
        Number.isNaN(fromLat) || Number.isNaN(fromLon) ||
        Number.isNaN(toLat) || Number.isNaN(toLon)
    ) {
        alert("Merci de choisir les adresses dans la liste de suggestions.");
        return;
    }

    const baseUrl = 'http://localhost:8080/ServerGPS/itinerary';
    const url = `${baseUrl}?fromLat=${fromLat}&fromLon=${fromLon}&toLat=${toLat}&toLon=${toLon}`;

    try {
        const res = await fetch(url);
        console.log('Itinéraire reçu :', res);
        if (!res.ok) {
            throw new Error(`Erreur HTTP ${res.status}`);
        }
        const data = await res.json();

        // TODO : afficher l’itinéraire sur la carte, ou dans la page
        console.log('Itinéraire reçu :', data);
    } catch (err) {
        console.error('Erreur lors de la récupération de l’itinéraire :', err);
        alert("Impossible de récupérer l’itinéraire.");
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
