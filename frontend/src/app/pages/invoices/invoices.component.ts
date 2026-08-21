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

  ngOnInit(): void { this.loadInvoices(); }
  protected loadInvoices(): void {
    this.loading.set(true); this.error.set('');
    this.billingService.listInvoices().subscribe({
      next: invoices => { this.invoices.set(invoices); this.loading.set(false); },
      error: () => { this.error.set('Não foi possível carregar as notas fiscais.'); this.loading.set(false); },
    });
  }
  protected statusLabel(status: InvoiceStatus): string { return status === 'Closed' || status === 1 ? 'Fechada' : 'Aberta'; }
}
