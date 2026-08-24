import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { InvoiceDetailComponent } from './invoice-detail.component';

describe('InvoiceDetailComponent', () => {
  let http: HttpTestingController;
  const openInvoice = {
    id: 'i1',
    number: 1,
    status: 1,
    createdAt: '2026-08-23T10:00:00Z',
    closedAt: null,
    items: [{ productId: 'p1', quantity: 2 }],
  };
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [InvoiceDetailComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'i1' } } } },
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());
  function createComponent() {
    const fixture = TestBed.createComponent(InvoiceDetailComponent);
    fixture.detectChanges();
    http.expectOne('/billing-api/invoices/i1').flush(openInvoice);
    http
      .expectOne('/api/products?page=1&pageSize=100')
      .flush({
        items: [{ id: 'p1', code: 'P-1', description: 'Produto 1', stock: 5 }],
        page: 1,
        pageSize: 100,
        totalItems: 1,
        totalPages: 1,
      });
    fixture.detectChanges();
    return fixture;
  }

  it('closes an open invoice and receives the closed status', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance as any;
    component.closeInvoice();
    const request = http.expectOne('/billing-api/invoices/i1/close');
    expect(request.request.method).toBe('POST');
    request.flush({ ...openInvoice, status: 2, closedAt: '2026-08-23T10:05:00Z' });
    expect(component.invoice().status).toBe(2);
  });

  it('keeps the invoice open when closing returns 503', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance as any;
    component.closeInvoice();
    http
      .expectOne('/billing-api/invoices/i1/close')
      .flush('unavailable', { status: 503, statusText: 'Service Unavailable' });
    expect(component.invoice().status).toBe(1);
    expect(component.error()).toContain('indisponível');
  });

  it('renders numeric Open status as Aberta', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance as any;
    expect(component.statusLabel(1)).toBe('Aberta');
    expect(component.statusLabel(2)).toBe('Fechada');
  });
});
