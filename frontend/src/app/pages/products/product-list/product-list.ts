
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
        this.products = data;
        this.loading = false;

        this.cdr.detectChanges();
      },

      error: (error) => {
        console.error('Failed to load products:', error);

        this.errorMessage =
          'Something went wrong while loading products. Please try again.';

        this.loading = false;

        this.cdr.detectChanges();
      }

    });
  }

  deleteProduct(id: number): void {

    const confirmed = confirm(
      'Are you sure you want to delete this product? This action cannot be undone.'
    );

    if (!confirmed) {
      return;
    }

    this.productService.delete(id).subscribe({

      next: () => {
        this.loadProducts();
      },

      error: (error) => {
        console.error('Failed to delete product:', error);

        this.errorMessage =
          'Something went wrong while deleting the product.';

        this.cdr.detectChanges();
      }

    });
  }
}
