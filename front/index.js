// === INITIALISATION LEAFLET ===

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

    // Suppression des anciens éléments
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
