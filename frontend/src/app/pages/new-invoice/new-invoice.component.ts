import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Product } from '../../models/product.model';
import { BillingService } from '../../services/billing.service';
import { StockService } from '../../services/stock.service';

interface DraftItem { product: Product; quantity: number; }

@Component({
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './new-invoice.component.html',
  styleUrl: './new-invoice.component.scss',
})
export class NewInvoiceComponent implements OnInit {
  private readonly stockService = inject(StockService);
  private readonly billingService = inject(BillingService);
  private readonly router = inject(Router);
  protected readonly products = signal<Product[]>([]);
  protected readonly items = signal<DraftItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal('');
  protected selectedProductId = '';
  protected quantity = 1;

  ngOnInit(): void {
    this.stockService.listProducts().subscribe({
      next: result => { this.products.set(result.items); this.loading.set(false); },
      error: () => { this.error.set('Não foi possível carregar os produtos.'); this.loading.set(false); },
    });
  }

  protected addItem(): void {
    const product = this.products().find(item => item.id === this.selectedProductId);
    if (!product || this.quantity < 1) { this.error.set('Selecione um produto e informe uma quantidade válida.'); return; }
    if (this.items().some(item => item.product.id === product.id)) { this.error.set('Este produto já foi adicionado à nota.'); return; }
    this.items.update(items => [...items, { product, quantity: this.quantity }]);
    this.selectedProductId = ''; this.quantity = 1; this.error.set('');
  }

  protected removeItem(productId: string): void { this.items.update(items => items.filter(item => item.product.id !== productId)); }

  protected createInvoice(): void {
    if (this.items().length === 0) { this.error.set('Adicione pelo menos um produto à nota.'); return; }
    this.saving.set(true); this.error.set('');
    this.billingService.createInvoice({ items: this.items().map(item => ({ productId: item.product.id, quantity: item.quantity })) }).subscribe({
      next: invoice => { this.saving.set(false); this.router.navigate(['/invoices', invoice.id]); },
      error: () => { this.saving.set(false); this.error.set('Não foi possível criar a nota fiscal.'); },
    });
  }
}
