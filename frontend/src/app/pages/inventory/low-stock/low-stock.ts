
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';

import { LowStockAlert } from '../../../models/inventory.model';
import { InventoryService } from '../../../services/inventory.service';

@Component({
  selector: 'app-low-stock',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './low-stock.html',
  styleUrl: './low-stock.scss',
})
export class LowStock implements OnInit {
  private inventoryService = inject(InventoryService);
  private cdr = inject(ChangeDetectorRef);

  alerts: LowStockAlert[] = [];
  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadAlerts();
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
        this.errorMessage = 'Düşük stok bilgileri yüklenirken hata oluştu.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
