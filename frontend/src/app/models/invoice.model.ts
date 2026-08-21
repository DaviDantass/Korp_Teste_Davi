export type InvoiceStatus = 'Open' | 'Closed' | 0 | 1;

export interface InvoiceItem {
  productId: string;
  quantity: number;
}

export interface Invoice {
  id: string;
  number: number;
  status: InvoiceStatus;
  createdAt: string;
  closedAt: string | null;
  items: InvoiceItem[];
}
