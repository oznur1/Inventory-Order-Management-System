import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  Inventory,
  CreateInventory,
  LowStockAlert,
  InventoryReport
} from '../models/inventory.model';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5057/api/Inventory';

  getAll(): Observable<Inventory[]> {
    return this.http.get<Inventory[]>(this.apiUrl);
  }

  getLowStock(): Observable<LowStockAlert[]> {
    return this.http.get<LowStockAlert[]>(
      `${this.apiUrl}/low-stock`
    );
  }

  getReport(): Observable<InventoryReport> {
    return this.http.get<InventoryReport>(
      `${this.apiUrl}/report`
    );
  }

  create(inventory: CreateInventory): Observable<Inventory> {
    return this.http.post<Inventory>(
      this.apiUrl,
      inventory
    );
  }
}