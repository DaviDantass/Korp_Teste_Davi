import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Product } from '../models/product.model';

export interface CreateProductRequest { code: string; description: string; initialStock: number; }
export interface UpdateProductRequest { description: string; }
export interface StockMovementRequest { quantity: number; }

@Injectable({ providedIn: 'root' })
export class StockService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  listProducts(): Observable<Product[]> { return this.http.get<Product[]>(`${this.baseUrl}/products`); }
  createProduct(request: CreateProductRequest): Observable<Product> { return this.http.post<Product>(`${this.baseUrl}/products`, request); }
  updateProduct(id: string, request: UpdateProductRequest): Observable<Product> { return this.http.put<Product>(`${this.baseUrl}/products/${id}`, request); }
  addStock(id: string, request: StockMovementRequest): Observable<Product> { return this.http.post<Product>(`${this.baseUrl}/stock/${id}/stock-in`, request); }
  withdrawStock(id: string, request: StockMovementRequest): Observable<Product> { return this.http.post<Product>(`${this.baseUrl}/stock/${id}/stock-out`, request); }
}
