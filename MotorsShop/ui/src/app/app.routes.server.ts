import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: 'motorcycles/:id',
    renderMode: RenderMode.Server,    // render on each request, not at build
  },
  {
    path: '**',
    renderMode: RenderMode.Prerender, // everything else prerendered
  },
];