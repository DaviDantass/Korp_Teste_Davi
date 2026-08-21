import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BillingService } from '../../services/billing.service';
import { Invoice, InvoiceStatus } from '../../models/invoice.model';

@Component({
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './invoice-detail.component.html',
  styleUrl: './invoice-detail.component.scss',
})
export class InvoiceDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly billingService = inject(BillingService);
  protected readonly invoice = signal<Invoice | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal('');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.error.set('Nota fiscal inválida.'); this.loading.set(false); return; }
    this.billingService.getInvoice(id).subscribe({
      next: invoice => { this.invoice.set(invoice); this.loading.set(false); },
      error: () => { this.error.set('Não foi possível carregar a nota fiscal.'); this.loading.set(false); },
    });
  }

  protected statusLabel(status: InvoiceStatus): string { return status === 'Closed' || status === 1 ? 'Fechada' : 'Aberta'; }
}
