self.addEventListener('install', async event => {
    console.log('Installing service worker...');
    self.skipWaiting();
});

self.addEventListener('fetch', event => {
    // For Blazor Server, we typically just pass through to the network
    // because the app relies on a live SignalR connection.
    return;
});
