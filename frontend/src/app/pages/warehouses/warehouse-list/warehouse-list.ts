
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

import { Warehouse } from '../../../models/warehouse.model';
import { WarehouseService } from '../../../services/warehouse.service';

@Component({
  selector: 'app-warehouse-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './warehouse-list.html',
  styleUrl: './warehouse-list.scss',
})
export class WarehouseList implements OnInit {

  private warehouseService = inject(WarehouseService);
  private cdr = inject(ChangeDetectorRef);
  private fb = inject(FormBuilder);

  warehouses: Warehouse[] = [];

  loading = false;
  saving = false;

  errorMessage = '';
  formErrorMessage = '';
  showCreateModal = false;

  warehouseForm = this.fb.nonNullable.group({
    code: ['', [
      Validators.required,
      Validators.maxLength(50)
    ]],
    name: ['', [
      Validators.required,
      Validators.maxLength(200)
    ]],
    address: ['']
  });

  ngOnInit(): void {
    this.loadWarehouses();
  }

  loadWarehouses(): void {
    this.loading = true;
    this.errorMessage = '';

    this.warehouseService.getAll().subscribe({
      next: (data) => {
        this.warehouses = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Failed to load warehouses:', error);

        this.errorMessage =
          'Something went wrong while loading warehouses. Please try again.';

        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openCreateModal(): void {
    this.warehouseForm.reset({
      code: '',
      name: '',
      address: ''
    });

    this.formErrorMessage = '';
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    if (this.saving) {
      return;
    }

    this.showCreateModal = false;
    this.formErrorMessage = '';
  }

  createWarehouse(): void {
    this.formErrorMessage = '';

    if (this.warehouseForm.invalid) {
      this.warehouseForm.markAllAsTouched();
      return;
    }

    const values = this.warehouseForm.getRawValue();

    const warehouse = {
      code: values.code.trim(),
      name: values.name.trim(),
      address: values.address.trim() || null
    };

    if (!warehouse.code || !warehouse.name) {
      this.warehouseForm.markAllAsTouched();
      return;
    }

    this.saving = true;

    this.warehouseService.create(warehouse).subscribe({
      next: () => {
        this.saving = false;
        this.showCreateModal = false;

        this.warehouseForm.reset({
          code: '',
          name: '',
          address: ''
        });

        this.loadWarehouses();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Failed to create warehouse:', error);

        this.formErrorMessage =
          error?.error?.detail ||
          error?.error?.message ||
          'Unable to create the warehouse. Please check the information and try again.';

        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }
}
