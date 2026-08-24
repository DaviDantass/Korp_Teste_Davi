import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NewInvoiceComponent } from './new-invoice.component';

describe('NewInvoiceComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [NewInvoiceComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: 'invoices/:id', redirectTo: '' }]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function createComponent() {
    const fixture = TestBed.createComponent(NewInvoiceComponent);
    fixture.detectChanges();
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

  it('loads products and adds/removes an item', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance as any;
    component.selectedProductId = 'p1';
    component.quantity = 2;
    component.addItem();
    expect(component.items()).toHaveLength(1);
    component.removeItem('p1');
    expect(component.items()).toHaveLength(0);
  });

  it('creates the invoice with the selected items', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance as any;
    component.selectedProductId = 'p1';
    component.quantity = 2;
    component.addItem();
    component.createInvoice();
    const request = http.expectOne('/billing-api/invoices');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ items: [{ productId: 'p1', quantity: 2 }] });
    request.flush({
      id: 'invoice-1',
      number: 1,
      status: 1,
      createdAt: new Date().toISOString(),
      closedAt: null,
      items: [],
    });
  });

  it('does not submit an empty invoice', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance as any;
    component.createInvoice();
    expect(component.error()).toContain('pelo menos um produto');
    http.expectNone('/billing-api/invoices');
  });
});
