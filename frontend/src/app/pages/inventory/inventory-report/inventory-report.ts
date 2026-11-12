
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';

import { InventoryService } from '../../../services/inventory.service';
import { StockMovementService } from '../../../services/stock-movement.service';

import {
  InventoryReport as InventoryReportModel
} from '../../../models/inventory.model';

import {
  StockMovement
} from '../../../models/stock-movement.model';

@Component({
  selector: 'app-inventory-report',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inventory-report.html',
  styleUrl: './inventory-report.scss'
})
export class InventoryReportComponent implements OnInit {

  private inventoryService = inject(InventoryService);
  private stockMovementService = inject(StockMovementService);
  private cdr = inject(ChangeDetectorRef);

  report: InventoryReportModel | null = null;
  movements: StockMovement[] = [];

  loading = false;
  errorMessage = '';

  get availabilityRate(): number {
    if (!this.report || this.report.totalQuantityOnHand <= 0) {
      return 0;
    }

    return Math.round(
      Math.max(
        0,
        Math.min(
          100,
          (this.report.totalAvailableQuantity /
            this.report.totalQuantityOnHand) * 100
        )
      )
    );
  }

  get reservedRate(): number {
    if (!this.report || this.report.totalQuantityOnHand <= 0) {
      return 0;
    }

    return Math.round(
      Math.max(
        0,
        Math.min(
          100,
          (this.report.totalReservedQuantity /
            this.report.totalQuantityOnHand) * 100
        )
      )
    );
  }

  get todayMovementCount(): number {
    const today = new Date();

    return this.movements.filter(movement => {
      return new Date(movement.createdAt).toDateString()
        === today.toDateString();
    }).length;
  }

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

  get recentMovements(): StockMovement[] {
    return [...this.movements]
      .sort(
        (a, b) =>
          new Date(b.createdAt).getTime() -
          new Date(a.createdAt).getTime()
      )
      .slice(0, 5);
  }

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    if (this.loading) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    let completedRequests = 0;
    let hasError = false;

    const finishRequest = (): void => {
      completedRequests++;

      if (completedRequests === 2) {
        this.loading = false;

        if (hasError) {
          this.errorMessage =
            'Dashboard verileri yüklenirken bir hata oluştu.';
        }

        this.cdr.markForCheck();
      }
    };

    this.inventoryService.getReport().subscribe({
      next: (data) => {
        this.report = data;
        finishRequest();
      },
      error: (error) => {
        console.error('[Dashboard] Inventory API error:', error);
        hasError = true;
        finishRequest();
      }
    });

    this.stockMovementService.getAll().subscribe({
      next: (data) => {
        this.movements = data;
        finishRequest();
      },
      error: (error) => {
        console.error('[Dashboard] Stock movements API error:', error);
        hasError = true;
        finishRequest();
      }
    });
  }
}
