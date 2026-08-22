import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ProductsComponent } from './products.component';

describe('ProductsComponent', () => {
  let http: HttpTestingController;
  const product = { id: 'p1', code: 'P-1', description: 'Produto 1', stock: 5 };

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [ProductsComponent], providers: [provideHttpClient(), provideHttpClientTesting()] });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());
  function createComponent() { const fixture = TestBed.createComponent(ProductsComponent); fixture.detectChanges(); http.expectOne('/api/products').flush([product]); fixture.detectChanges(); return fixture; }

  it('loads the product list from the StockService', () => {
    const fixture = createComponent();
    expect((fixture.componentInstance as any).products()).toEqual([product]);
  });

  it('creates a product through the API', () => {
    const fixture = createComponent(); const component = fixture.componentInstance as any;
    component.openCreate(); component.code = 'P-2'; component.description = 'Produto 2'; component.initialStock = 8; component.save();
    const request = http.expectOne('/api/products');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ code: 'P-2', description: 'Produto 2', initialStock: 8 });
    request.flush({ ...product, id: 'p2', code: 'P-2', description: 'Produto 2', stock: 8 });
    http.expectOne('/api/products').flush([product, { ...product, id: 'p2', code: 'P-2', description: 'Produto 2', stock: 8 }]);
  });

  it('updates the product description through the API', () => {
    const fixture = createComponent(); const component = fixture.componentInstance as any;
    component.openEdit(product); component.description = 'Produto atualizado'; component.save();
    const request = http.expectOne('/api/products/p1');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ description: 'Produto atualizado' });
    request.flush({ ...product, description: 'Produto atualizado' });
    http.expectOne('/api/products').flush([{ ...product, description: 'Produto atualizado' }]);
  });

  it('sends stock entry and reloads the persisted list', () => {
    const fixture = createComponent(); const component = fixture.componentInstance as any;
    component.openMovement(product, 'in'); component.initialStock = 3; component.saveMovement();
    const request = http.expectOne('/api/stock/p1/stock-in');
    expect(request.request.method).toBe('POST'); expect(request.request.body).toEqual({ quantity: 3 });
    request.flush({ ...product, stock: 8 });
    http.expectOne('/api/products').flush([{ ...product, stock: 8 }]);
  });

  it('sends stock withdrawal and reloads the persisted list', () => {
    const fixture = createComponent(); const component = fixture.componentInstance as any;
    component.openMovement(product, 'out'); component.initialStock = 2; component.saveMovement();
    const request = http.expectOne('/api/stock/p1/stock-out');
    expect(request.request.method).toBe('POST'); expect(request.request.body).toEqual({ quantity: 2 });
    request.flush({ ...product, stock: 3 });
    http.expectOne('/api/products').flush([{ ...product, stock: 3 }]);
  });
});
