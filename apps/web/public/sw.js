const CACHE='tutien-web-v1';
self.addEventListener('install',e=>e.waitUntil(caches.open(CACHE)));
self.addEventListener('fetch',e=>{
  if(e.request.method!=='GET')return;
  const url=new URL(e.request.url);
  if(url.pathname.startsWith('/api'))return;
});
