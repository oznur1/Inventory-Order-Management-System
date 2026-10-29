
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ProductService } from '../../../services/product.service';
import {
  CreateProduct,
  Product,
  UpdateProduct
} from '../../../models/product.model';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss',
})
export class ProductForm {
  @Input() productToEdit: Product | null = null;

  @Output() productCreated = new EventEmitter<void>();
  @Output() productUpdated = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private productService = inject(ProductService);

  loading = false;
  successMessage = '';
  errorMessage = '';
  isEditMode = false;

  productForm = this.fb.nonNullable.group({
    sku: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    unitPrice: [
      0,
      [Validators.required, Validators.min(0), Validators.max(999999999)]
    ],
  });

  loadProduct(product: Product): void {
    this.productToEdit = product;
    this.isEditMode = true;

    this.productForm.patchValue({
      sku: product.sku,
      name: product.name,
      description: product.description ?? '',
      unitPrice: product.unitPrice,
    });

    this.successMessage = '';
    this.errorMessage = '';
  }

  onSubmit(): void {
    this.successMessage = '';
    this.errorMessage = '';

    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.loading = true;

    const values = this.productForm.getRawValue();

    if (this.isEditMode && this.productToEdit) {
      const product: UpdateProduct = {
        ...values,
        isActive: this.productToEdit.isActive,
      };

      this.productService.update(this.productToEdit.id, product).subscribe({
        next: () => {
          this.successMessage = 'Ürün başarıyla güncellendi.';
          this.productUpdated.emit();
          this.resetForm();
        },
        error: (error) => {
          console.error('Ürün güncellenemedi:', error);
          this.errorMessage = 'Ürün güncellenirken bir hata oluştu.';
          this.loading = false;
        },
      });
    } else {
      const product: CreateProduct = values;

      this.productService.create(product).subscribe({
        next: () => {
          this.successMessage = 'Ürün başarıyla eklendi.';
          this.productCreated.emit();
          this.resetForm();
        },
        error: (error) => {
          console.error('Ürün eklenemedi:', error);
          this.errorMessage = 'Ürün eklenirken bir hata oluştu.';
          this.loading = false;
        },
      });
    }
  }

   cancelEdit(): void {
  this.resetForm();
  this.successMessage = '';
  this.errorMessage = '';
}

  private resetForm(): void {
    this.productForm.reset({
      sku: '',
      name: '',
      description: '',
      unitPrice: 0,
    });

    this.productToEdit = null;
    this.isEditMode = false;
    this.loading = false;
  }
}
