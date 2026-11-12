
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { StockMovementService } from '../../../services/stock-movement.service';
import { ProductService } from '../../../services/product.service';
import { WarehouseService } from '../../../services/warehouse.service';

import {
  StockMovement,
  StockMovementFilters,
  CreateStockMovement
} from '../../../models/stock-movement.model';

import { Product } from '../../../models/product.model';
import { Warehouse } from '../../../models/warehouse.model';

@Component({
  selector: 'app-stock-movements',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock-movements.html',
  styleUrl: './stock-movement.scss'
})
export class StockMovementsComponent implements OnInit {

  private stockMovementService = inject(StockMovementService);
  private productService = inject(ProductService);
  private warehouseService = inject(WarehouseService);

  movements: StockMovement[] = [];
  products: Product[] = [];
  warehouses: Warehouse[] = [];

  loading = false;
  loadingOptions = false;
  saving = false;

  errorMessage = '';
  formErrorMessage = '';
  successMessage = '';

  showMovementModal = false;

  productId: number | null = null;
  warehouseId: number | null = null;
  movementType = '';
  from = '';
  to = '';

  newMovement: CreateStockMovement = this.getEmptyMovement();

  get inboundCount(): number {
    return this.movements.filter(
      movement => movement.movementType === 'Inbound'
    ).length;
  }

  get outboundCount(): number {
    return this.movements.filter(
      movement => movement.movementType === 'Outbound'
    ).length;
  }

  ngOnInit(): void {
    this.loadMovements();
    this.loadFormOptions();
  }

  getEmptyMovement(): CreateStockMovement {
    return {
      productId: 0,
      warehouseId: 0,
      movementType: 'Inbound',
      quantity: 1,
      referenceNumber: null,
      notes: null
    };
  }

  loadMovements(): void {
    this.loading = true;
    this.errorMessage = '';

    const filters: StockMovementFilters = {
      productId: this.productId || undefined,
      warehouseId: this.warehouseId || undefined,
      movementType: this.movementType
        ? this.movementType as 'Inbound' | 'Outbound'
        : undefined,
      from: this.from || undefined,
      to: this.to || undefined
    };

    this.stockMovementService.getAll(filters).subscribe({
      next: (data) => {
        this.movements = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Stock movements API error:', error);
        this.errorMessage =
          'Stock movements could not be loaded. Please check your connection and try again.';
        this.loading = false;
      }
    });
  }

  loadFormOptions(): void {
    this.loadingOptions = true;

    this.productService.getAll().subscribe({
      next: (data) => {
        this.products = data.filter(product => product.isActive);
        this.loadingOptions = false;
      },
      error: (error) => {
        console.error('Products could not be loaded:', error);
        this.loadingOptions = false;
      }
    });

    this.warehouseService.getAll().subscribe({
      next: (data) => {
        this.warehouses = data.filter(warehouse => warehouse.isActive);
      },
      error: (error) => {
        console.error('Warehouses could not be loaded:', error);
      }
    });
  }

  openMovementModal(): void {
    this.newMovement = this.getEmptyMovement();
    this.formErrorMessage = '';
    this.successMessage = '';
    this.showMovementModal = true;
  }

  closeMovementModal(): void {
    if (this.saving) {
      return;
    }

    this.showMovementModal = false;
    this.formErrorMessage = '';
  }

  createMovement(): void {
    this.formErrorMessage = '';
    this.successMessage = '';

    if (
      this.newMovement.productId < 1 ||
      this.newMovement.warehouseId < 1 ||
      this.newMovement.quantity < 1
    ) {
      this.formErrorMessage =
        'Please select a product and warehouse and enter a valid quantity.';
      return;
    }

    this.saving = true;

    const movement: CreateStockMovement = {
      ...this.newMovement,
      referenceNumber: this.newMovement.referenceNumber?.trim() || null,
      notes: this.newMovement.notes?.trim() || null
    };

    this.stockMovementService.create(movement).subscribe({
      next: () => {
        this.saving = false;
        this.showMovementModal = false;
        this.successMessage = 'Stock movement successfully created.';

        this.loadMovements();
      },
      error: (error) => {
        console.error('Stock movement creation failed:', error);

        this.formErrorMessage =
          error.error?.message ??
          'Stock movement could not be created. Please check the available stock and try again.';

        this.saving = false;
      }
    });
  }

  applyFilters(): void {
    this.loadMovements();
  }

  resetFilters(): void {
    this.productId = null;
    this.warehouseId = null;
    this.movementType = '';
    this.from = '';
    this.to = '';

    this.loadMovements();
  }
}
