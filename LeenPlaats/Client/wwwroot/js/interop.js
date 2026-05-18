// wwwroot/js/interop.js

window.setTheme = (theme) => {
  document.documentElement.setAttribute('data-theme', theme);
  localStorage.setItem('theme', theme);
};

window.getTheme = () => localStorage.getItem('theme') || 'light';

window.getLocation = () => new Promise((resolve, reject) =>
  navigator.geolocation.getCurrentPosition(
    p => resolve({ lat: p.coords.latitude, lng: p.coords.longitude }),
    e => reject(e.message)
  )
);

window.initMap = (elementId, lat, lng) => {
    const el = document.getElementById(elementId);
    if (!el) return;
    const map = L.map(el, {
        zoomControl: false,
        dragging: false,
        scrollWheelZoom: false,
        doubleClickZoom: false,
        boxZoom: false,
        keyboard: false
    });
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);
    const circle = L.circle([lat, lng], {
        radius: 1000,
        color: '#2d6a4f',
        fillColor: '#52b788',
        fillOpacity: 0.2,
        weight: 2
    }).addTo(map);
    map.fitBounds(circle.getBounds());
};
