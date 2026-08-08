import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: 'motorcycles/:id',
    renderMode: RenderMode.Server,    // render on each request, not at build
  },

  // Authenticated routes must NOT be prerendered or SSR-rendered:
  // the guard reads localStorage, which only exists in the browser. If we
  // prerender these at build time, the guard sees "not authenticated" and
  // redirects to /login, and that redirected HTML is what the browser gets
  // on a hard refresh. Rendering client-only lets the guard run after
  // hydration where the session token is available.
  { path: 'me',         renderMode: RenderMode.Client },
  { path: 'my-orders',  renderMode: RenderMode.Client },
  { path: 'order',      renderMode: RenderMode.Client },
  { path: 'admin',      renderMode: RenderMode.Client },
  { path: 'admin/**',   renderMode: RenderMode.Client },

  {
    path: '**',
    renderMode: RenderMode.Prerender, // everything else prerendered
  },
];
