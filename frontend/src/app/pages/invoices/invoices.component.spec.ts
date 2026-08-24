import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { InvoicesComponent } from './invoices.component';

describe('InvoicesComponent', () => {
  let http: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ imports: [InvoicesComponent], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] }); http = TestBed.inject(HttpTestingController); });
  afterEach(() => http.verify());

  it('loads invoices from the BillingService', () => {
    const fixture = TestBed.createComponent(InvoicesComponent); fixture.detectChanges();
    const invoices = [{ id: 'i1', number: 1, status: 1, createdAt: '2026-08-22T10:00:00Z', closedAt: null, items: [{ productId: 'p1', quantity: 2 }] }];
    http.expectOne('/billing-api/invoices?page=1&pageSize=10').flush({ items: invoices, page: 1, pageSize: 10, totalItems: 1, totalPages: 1 });
    expect((fixture.componentInstance as any).invoices()).toEqual(invoices);
  });

  it('shows an error when BillingService is unavailable', () => {
    const fixture = TestBed.createComponent(InvoicesComponent); fixture.detectChanges();
    http.expectOne('/billing-api/invoices?page=1&pageSize=10').flush('unavailable', { status: 503, statusText: 'Service Unavailable' });
    expect((fixture.componentInstance as any).error()).toContain('Não foi possível');
  });
});
