
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { StockMovementService } from '../../../services/stock-movement.service';

import {
  StockMovement,
  StockMovementFilters
} from '../../../models/stock-movement.model';

@Component({
  selector: 'app-stock-movements',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock-movements.html',
styleUrl: './stock-movement.scss'
})
export class StockMovementsComponent implements OnInit {

  private stockMovementService = inject(StockMovementService);

  movements: StockMovement[] = [];

  loading = false;
  errorMessage = '';

  productId: number | null = null;
  warehouseId: number | null = null;
  movementType = '';
  from = '';
  to = '';

  ngOnInit(): void {
    this.loadMovements();
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
          'Stock movements could not be loaded. Please try again.';

        this.loading = false;
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
