import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { Invoice, InvoiceStatus } from '../../models/invoice.model';
import { BillingService } from '../../services/billing.service';

@Component({
  standalone: true,
  imports: [RouterLink, DatePipe],
  templateUrl: './invoices.component.html',
  styleUrl: './invoices.component.scss',
})
export class InvoicesComponent implements OnInit {
  private readonly billingService = inject(BillingService);
  protected readonly invoices = signal<Invoice[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly page = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalItems = signal(0);

  ngOnInit(): void {
    this.loadInvoices();
  }
  protected loadInvoices(): void {
    this.loading.set(true);
    this.error.set('');
    this.billingService.listInvoices(this.page(), 10).subscribe({
      next: (result) => {
        this.invoices.set(result.items);
        this.totalPages.set(result.totalPages);
        this.totalItems.set(result.totalItems);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar as notas fiscais.');
        this.loading.set(false);
      },
    });
  }
  protected previousPage(): void {
    if (this.page() > 1) {
      this.page.update((value) => value - 1);
      this.loadInvoices();
    }
  }
  protected nextPage(): void {
    if (this.page() < this.totalPages()) {
      this.page.update((value) => value + 1);
      this.loadInvoices();
    }
  }
  protected statusLabel(status: InvoiceStatus): string {
    return status === 'Closed' || status === 2 ? 'Fechada' : 'Aberta';
  }
}
