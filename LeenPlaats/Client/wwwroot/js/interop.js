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
