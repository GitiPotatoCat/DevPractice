import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./admin-layout/admin-layout.component').then(m => m.AdminLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./admin-home/admin-home.component').then(m => m.AdminHomeComponent),
      },
      {
        path: 'brands',
        loadComponent: () =>
          import('./brands/admin-brands.component').then(m => m.AdminBrandsComponent),
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./categories/admin-categories.component').then(m => m.AdminCategoriesComponent),
      },
      {
        path: 'motorcycles',
        loadComponent: () =>
          import('./motorcycles/admin-motorcycles.component').then(m => m.AdminMotorcyclesComponent),
      },
      {
        path: 'customers',
        loadComponent: () =>
          import('./customers/admin-customers.component').then(m => m.AdminCustomersComponent),
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./orders/admin-orders.component').then(m => m.AdminOrdersComponent),
      },
    ],
  },
];