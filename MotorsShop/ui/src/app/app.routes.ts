// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
    // Public
    {
        path: '',
        loadComponent: () =>
            import('./features/motorcycles/motorcycle-list/motorcycle-list.component')
                .then(m => m.MotorcycleListComponent),
    },
    {
        path: 'motorcycles/:id',
        loadComponent: () =>
            import('./features/motorcycles/motorcycle-detail/motorcycle-detail.component')
                .then(m => m.MotorcycleDetailComponent),
    },
    {
        path: 'login',
        loadComponent: () =>
            import('./features/auth/login/login.component')
                .then(m => m.LoginComponent),
    },
    {
        path: 'register',
        loadComponent: () =>
            import('./features/auth/register/register.component')
                .then(m => m.RegisterComponent),
    },
    {
        path: 'forgot-password',
        loadComponent: () =>
            import('./features/auth/forgot-password/forgot-password.component')
                .then(m => m.ForgotPasswordComponent),
    },
    {
        path: 'reset-password',
        loadComponent: () =>
            import('./features/auth/reset-password/reset-password.component')
                .then(m => m.ResetPasswordComponent),
    },

    // Customer (authenticated)
    {
        path: 'me',
        canActivate: [authGuard],
        loadComponent: () =>
            import('./features/customer/profile/profile.component')
                .then(m => m.ProfileComponent),
    },
    {
        path: 'my-orders',
        canActivate: [authGuard],
        loadComponent: () =>
            import('./features/customer/my-orders/my-orders.component')
                .then(m => m.MyOrdersComponent),
    },
    {
        path: 'order',
        canActivate: [authGuard],
        loadComponent: () =>
            import('./features/customer/place-order/place-order.component')
                .then(m => m.PlaceOrderComponent),
    },

    // Admin
    {
        path: 'admin',
        canActivate: [roleGuard('Admin')],
        loadChildren: () =>
            import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES),
    },

    { path: '**', redirectTo: '' },
];