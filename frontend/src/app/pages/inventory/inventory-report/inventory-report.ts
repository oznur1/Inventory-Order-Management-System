
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';

import { InventoryService } from '../../../services/inventory.service';
import { InventoryReport as InventoryReportModel } from '../../../models/inventory.model';

@Component({
  selector: 'app-inventory-report',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inventory-report.html',
  styleUrl: './inventory-report.scss'
})
export class InventoryReportComponent implements OnInit {

  private inventoryService = inject(InventoryService);
  private cdr = inject(ChangeDetectorRef);

  report: InventoryReportModel | null = null;
  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    console.log('[InventoryReport] Component initialized');
    this.loadReport();
  }

  loadReport(): void {
    console.log('[InventoryReport] API request starting');

    this.loading = true;
    this.errorMessage = '';

    this.inventoryService.getReport().subscribe({
      next: (data) => {
        console.log('[InventoryReport] API success:', data);

        this.report = data;
        this.loading = false;

        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('[InventoryReport] API error:', error);

        this.errorMessage =
          'Inventory report could not be loaded. Please try again.';

        this.loading = false;

        this.cdr.markForCheck();
      },

      complete: () => {
        console.log('[InventoryReport] Request completed');
      }
    });
  }
}
