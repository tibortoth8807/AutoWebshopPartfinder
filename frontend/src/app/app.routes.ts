import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'PartFinder – alkatrészkereső',
    loadComponent: () => import('./pages/home.page').then((m) => m.HomePage),
  },
  {
    path: 'alkatreszek',
    title: 'Alkatrészek – PartFinder',
    loadComponent: () => import('./pages/parts.page').then((m) => m.PartsPage),
  },
  {
    path: 'kosar',
    title: 'Kosár – PartFinder',
    loadComponent: () => import('./pages/cart.page').then((m) => m.CartPage),
  },
  { path: '**', redirectTo: '' },
];
