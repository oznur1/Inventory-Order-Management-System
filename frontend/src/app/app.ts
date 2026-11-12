
import { Component, ViewChild } from '@angular/core';

import { ProductList } from './pages/products/product-list/product-list';
import { ProductForm } from './pages/products/product-form/product-form';
import { WarehouseList } from './pages/warehouses/warehouse-list/warehouse-list';
import { LowStock } from './pages/inventory/low-stock/low-stock';
import { InventoryReportComponent } from './pages/inventory/inventory-report/inventory-report';
import { StockMovementsComponent } from './pages/inventory/stock-movements/stock-movement';

import { Product } from './models/product.model';

type Page =
  | 'dashboard'
  | 'products'
  | 'warehouses'
  | 'inventory'
  | 'stock-movements';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    ProductList,
    ProductForm,
    WarehouseList,
    LowStock,
    InventoryReportComponent,
    StockMovementsComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {

  @ViewChild(ProductList)
  productList!: ProductList;

  @ViewChild(ProductForm)
  productForm?: ProductForm;

  activePage: Page = 'dashboard';

  showProductModal = false;

  navigateTo(page: Page): void {
    this.activePage = page;
  }

  openCreateProduct(): void {
    this.productForm?.cancelEdit();
    this.showProductModal = true;
  }

  editProduct(product: Product): void {
    this.showProductModal = true;

    setTimeout(() => {
      this.productForm?.loadProduct(product);
    });
  }

  closeProductModal(): void {
    this.productForm?.cancelEdit();
    this.showProductModal = false;
  }

  onProductSaved(): void {
    this.productList?.loadProducts();
    this.showProductModal = false;
  }
}
