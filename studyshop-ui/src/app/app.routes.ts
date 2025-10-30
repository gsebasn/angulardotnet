import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/products',
    pathMatch: 'full'
  },
  {
    path: 'products',
    loadComponent: () => import('./pages/products-page/products-page.component').then(m => m.ProductsPageComponent)
  },
  {
    path: 'orders',
    loadComponent: () => import('./pages/orders-page/orders-page.component').then(m => m.OrdersPageComponent)
  }
];

