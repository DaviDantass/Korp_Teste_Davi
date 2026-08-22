import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Invoice } from '../models/invoice.model';

export interface CreateInvoiceItemRequest { productId: string; quantity: number; }
export interface CreateInvoiceRequest { items: CreateInvoiceItemRequest[]; }

@Injectable({ providedIn: 'root' })
export class BillingService {
  private readonly http = inject(HttpClient);
  listInvoices(): Observable<Invoice[]> { return this.http.get<Invoice[]>('/billing-api/invoices'); }
  getInvoice(id: string): Observable<Invoice> { return this.http.get<Invoice>(`/billing-api/invoices/${id}`); }
  createInvoice(request: CreateInvoiceRequest): Observable<Invoice> { return this.http.post<Invoice>('/billing-api/invoices', request); }
  closeInvoice(id: string): Observable<Invoice> { return this.http.post<Invoice>(`/billing-api/invoices/${id}/close`, {}); }
}
