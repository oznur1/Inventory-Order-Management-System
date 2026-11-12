
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { LowStockAlert } from '../../../models/inventory.model';
import { Product } from '../../../models/product.model';
import { Warehouse } from '../../../models/warehouse.model';

import { InventoryService } from '../../../services/inventory.service';
import { ProductService } from '../../../services/product.service';
import { WarehouseService } from '../../../services/warehouse.service';

@Component({
  selector: 'app-low-stock',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './low-stock.html',
  styleUrl: './low-stock.scss',
})
export class LowStock implements OnInit {

  private inventoryService = inject(InventoryService);
  private productService = inject(ProductService);
  private warehouseService = inject(WarehouseService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  alerts: LowStockAlert[] = [];
  products: Product[] = [];
  warehouses: Warehouse[] = [];

  loading = false;
  loadingOptions = false;
  saving = false;

  errorMessage = '';
  formErrorMessage = '';
  successMessage = '';

  showInventoryModal = false;

  inventoryForm = this.fb.nonNullable.group({
    productId: [0, [
      Validators.required,
      Validators.min(1)
    ]],
    warehouseId: [0, [
      Validators.required,
      Validators.min(1)
    ]],
    quantityOnHand: [0, [
      Validators.required,
      Validators.min(0)
    ]],
    reorderLevel: [0, [
      Validators.required,
      Validators.min(0)
    ]]
  });

  ngOnInit(): void {
    this.loadAlerts();
    this.loadFormOptions();
  }

  loadAlerts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.inventoryService.getLowStock().subscribe({
      next: (data) => {
        this.alerts = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Low stock API error:', error);

        this.errorMessage =
          'Something went wrong while loading low stock alerts. Please try again.';

        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadFormOptions(): void {
    this.loadingOptions = true;

    this.productService.getAll().subscribe({
      next: (data) => {
        this.products = data.filter(product => product.isActive);
        this.loadingOptions = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Products could not be loaded:', error);
        this.loadingOptions = false;
        this.cdr.detectChanges();
      }
    });

    this.warehouseService.getAll().subscribe({
      next: (data) => {
        this.warehouses = data.filter(warehouse => warehouse.isActive);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Warehouses could not be loaded:', error);
        this.cdr.detectChanges();
      }
    });
  }

  openInventoryModal(): void {
    this.formErrorMessage = '';
    this.successMessage = '';

    this.inventoryForm.reset({
      productId: 0,
      warehouseId: 0,
      quantityOnHand: 0,
      reorderLevel: 0
    });

    this.showInventoryModal = true;
  }

  closeInventoryModal(): void {
    if (this.saving) {
      return;
    }

    this.showInventoryModal = false;
    this.formErrorMessage = '';
  }

  createInventory(): void {
    this.formErrorMessage = '';
    this.successMessage = '';

    if (this.inventoryForm.invalid) {
      this.inventoryForm.markAllAsTouched();
      return;
    }

    if (this.products.length === 0 || this.warehouses.length === 0) {
      this.formErrorMessage =
        'An active product and an active warehouse are required.';
      return;
    }

    this.saving = true;

    const inventory = this.inventoryForm.getRawValue();

    this.inventoryService.create(inventory).subscribe({
      next: () => {
        this.saving = false;
        this.showInventoryModal = false;
        this.successMessage = 'Inventory record successfully created.';

        this.loadAlerts();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Inventory creation failed:', error);

        this.formErrorMessage =
          error.error?.message ??
          'Something went wrong while creating the inventory record.';

        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }
}
