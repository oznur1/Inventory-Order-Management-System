import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  LowStockAlert,
  InventoryReport
} from '../models/inventory.model';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5057/api/Inventory';

  getLowStock(): Observable<LowStockAlert[]> {
    return this.http.get<LowStockAlert[]>(`${this.apiUrl}/low-stock`);
  }

  getReport(): Observable<InventoryReport> {
    return this.http.get<InventoryReport>(`${this.apiUrl}/report`);
  }
}