
import { Component, signal, ViewChild } from '@angular/core';

import { ProductList } from './pages/products/product-list/product-list';
import { WarehouseList } from './pages/warehouses/warehouse-list/warehouse-list';
import { ProductForm } from './pages/products/product-form/product-form';
import { LowStock } from './pages/inventory/low-stock/low-stock';

@Component({
  selector: 'app-root',
  imports: [ProductList, WarehouseList, ProductForm, LowStock],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('frontend');

  @ViewChild(ProductForm) productForm!: ProductForm;

  editProduct(product: import('./models/product.model').Product): void {
    this.productForm.loadProduct(product);
  }
}
