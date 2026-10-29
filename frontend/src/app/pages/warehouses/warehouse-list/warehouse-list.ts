
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { Warehouse } from '../../../models/warehouse.model';
import { WarehouseService } from '../../../services/warehouse.service';

@Component({
  selector: 'app-warehouse-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './warehouse-list.html',
  styleUrl: './warehouse-list.scss',
})
export class WarehouseList implements OnInit {
  private warehouseService = inject(WarehouseService);
  private cdr = inject(ChangeDetectorRef);

  warehouses: Warehouse[] = [];
  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadWarehouses();
  }

  loadWarehouses(): void {
    this.loading = true;
    this.errorMessage = '';

    this.warehouseService.getAll().subscribe({
      next: (data) => {
        console.log('API depo yanıtı:', data);

        this.warehouses = data;
        this.loading = false;

        console.log('Depo sayısı:', this.warehouses.length);

        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Depolar yüklenemedi:', error);

        this.errorMessage = 'Depolar yüklenirken bir hata oluştu.';
        this.loading = false;

        this.cdr.detectChanges();
      },
    });
  }
}
