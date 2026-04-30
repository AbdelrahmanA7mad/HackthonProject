const CACHE_NAME = 'namaa-v1';

// نحتاج فقط لتسجيل الـ Service Worker لتمكين ميزة التثبيت (Installable)
self.addEventListener('install', (event) => {
    self.skipWaiting();
});

self.addEventListener('fetch', (event) => {
    // يمكن إضافة منطق التخزين المؤقت هنا لاحقاً إذا لزم الأمر
    event.respondWith(fetch(event.request));
});
