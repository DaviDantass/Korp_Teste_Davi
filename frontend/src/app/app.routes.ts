import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'products',
    loadComponent: () =>
      import('./pages/products/products.component').then((m) => m.ProductsComponent),
  },
  {
    path: 'invoices',
    loadComponent: () =>
      import('./pages/invoices/invoices.component').then((m) => m.InvoicesComponent),
  },
  {
    path: 'invoices/new',
    loadComponent: () =>
      import('./pages/new-invoice/new-invoice.component').then((m) => m.NewInvoiceComponent),
  },
  {
    path: 'invoices/:id',
    loadComponent: () =>
      import('./pages/invoice-detail/invoice-detail.component').then(
        (m) => m.InvoiceDetailComponent,
      ),
  },
  { path: '**', redirectTo: '' },
];
