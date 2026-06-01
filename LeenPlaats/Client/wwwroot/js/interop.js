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
    const map = L.map(el);
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
    map.setView([lat, lng], 13);
};

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    return Uint8Array.from([...rawData].map(c => c.charCodeAt(0)));
}

window.subscribeToPush = async (vapidPublicKey) => {
    if (!('Notification' in window) || !('serviceWorker' in navigator)) return null;
    const permission = await Notification.requestPermission();
    if (permission !== 'granted') return null;
    const registration = await navigator.serviceWorker.ready;
    const subscription = await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
    });
    return JSON.stringify(subscription);
};

window.unsubscribeFromPush = async () => {
    if (!('serviceWorker' in navigator)) return;
    const registration = await navigator.serviceWorker.ready;
    const subscription = await registration.pushManager.getSubscription();
    if (subscription) await subscription.unsubscribe();
};
