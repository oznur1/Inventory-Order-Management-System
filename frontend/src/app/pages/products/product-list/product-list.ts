
import {
  Component,
  OnInit,
  EventEmitter,
  Output,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Product } from '../../../models/product.model';
import { ProductService } from '../../../services/product.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
})
export class ProductList implements OnInit {
  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

  @Output() editRequested = new EventEmitter<Product>();

  products: Product[] = [];
  loading = false;
  errorMessage = '';
  searchText = '';

  ngOnInit(): void {
    this.loadProducts();
  }

  get filteredProducts(): Product[] {
    const search = this.searchText.trim().toLowerCase();

    if (!search) {
      return this.products;
    }

    return this.products.filter(product =>
      product.name.toLowerCase().includes(search) ||
      product.sku.toLowerCase().includes(search)
    );
  }

  loadProducts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService.getAll().subscribe({
      next: (data) => {
        console.log('API ürün yanıtı:', data);

        this.products = data;
        this.loading = false;

        console.log('Loading:', this.loading);
        console.log('Ürün sayısı:', this.products.length);

        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Ürün API hatası:', error);

        this.errorMessage = 'Ürünler yüklenirken bir hata oluştu.';
        this.loading = false;

        this.cdr.detectChanges();
      },
    });
  }

  deleteProduct(id: number): void {
    const confirmed = confirm(
      'Bu ürünü silmek istediğine emin misin?'
    );

    if (!confirmed) {
      return;
    }

    this.productService.delete(id).subscribe({
      next: () => {
        this.loadProducts();
      },
      error: (error) => {
        console.error('Ürün silinemedi:', error);
        this.errorMessage = 'Ürün silinirken bir hata oluştu.';
        this.cdr.detectChanges();
      },
    });
  }
}
