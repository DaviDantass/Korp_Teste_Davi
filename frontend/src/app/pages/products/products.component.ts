import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Product } from '../../models/product.model';
import { StockService } from '../../services/stock.service';

@Component({
  standalone: true,
  imports: [FormsModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss',
})
export class ProductsComponent implements OnInit {
  private readonly stockService = inject(StockService);
  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal('');
  protected readonly page = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalItems = signal(0);
  protected search = '';
  protected readonly formOpen = signal(false);
  protected readonly editing = signal<Product | null>(null);
  protected readonly movement = signal<'in' | 'out' | null>(null);
  protected code = '';
  protected description = '';
  protected initialStock = 0;

  ngOnInit(): void {
    this.loadProducts();
  }

  protected loadProducts(): void {
    this.loading.set(true);
    this.error.set('');
    this.stockService.listProducts(this.page(), 10, this.search).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalPages.set(result.totalPages);
        this.totalItems.set(result.totalItems);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar os produtos.');
        this.loading.set(false);
      },
    });
  }
  protected searchProducts(): void {
    this.page.set(1);
    this.loadProducts();
  }
  protected previousPage(): void {
    if (this.page() > 1) {
      this.page.update((value) => value - 1);
      this.loadProducts();
    }
  }
  protected nextPage(): void {
    if (this.page() < this.totalPages()) {
      this.page.update((value) => value + 1);
      this.loadProducts();
    }
  }

  protected openCreate(): void {
    this.code = '';
    this.description = '';
    this.initialStock = 0;
    this.error.set('');
    this.formOpen.set(true);
  }
  protected openEdit(product: Product): void {
    this.editing.set(product);
    this.description = product.description;
    this.error.set('');
  }
  protected closeForms(): void {
    if (!this.saving()) {
      this.formOpen.set(false);
      this.editing.set(null);
    }
  }
  protected openMovement(product: Product, type: 'in' | 'out'): void {
    this.editing.set(product);
    this.movement.set(type);
    this.initialStock = 0;
    this.error.set('');
  }
  protected closeMovement(): void {
    if (!this.saving()) {
      this.movement.set(null);
      this.editing.set(null);
    }
  }

  protected save(): void {
    if (
      !this.description.trim() ||
      this.initialStock < 0 ||
      (!this.editing() && !this.code.trim())
    ) {
      this.error.set('Preencha os campos obrigatórios.');
      return;
    }
    this.saving.set(true);
    this.error.set('');
    const request = this.editing()
      ? this.stockService.updateProduct(this.editing()!.id, {
          description: this.description.trim(),
        })
      : this.stockService.createProduct({
          code: this.code.trim(),
          description: this.description.trim(),
          initialStock: this.initialStock,
        });
    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeForms();
        this.loadProducts();
      },
      error: (response) => {
        this.saving.set(false);
        this.error.set(
          response.status === 409
            ? 'Já existe um produto com este código.'
            : 'Não foi possível salvar o produto.',
        );
      },
    });
  }

  protected saveMovement(): void {
    const product = this.editing();
    if (!product || this.initialStock < 1) {
      this.error.set('Informe uma quantidade maior que zero.');
      return;
    }
    this.saving.set(true);
    this.error.set('');
    const request = { quantity: this.initialStock };
    const operation =
      this.movement() === 'in'
        ? this.stockService.addStock(product.id, request)
        : this.stockService.withdrawStock(product.id, request);
    operation.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeMovement();
        this.loadProducts();
      },
      error: (response) => {
        this.saving.set(false);
        this.error.set(
          response.status === 409
            ? 'Saldo insuficiente para realizar a baixa.'
            : 'Não foi possível atualizar o estoque.',
        );
      },
    });
  }
}
