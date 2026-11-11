
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  StockMovement,
  StockMovementFilters,
  StockMovementReport
} from '../models/stock-movement.model';

@Injectable({
  providedIn: 'root'
})
export class StockMovementService {

  private http = inject(HttpClient);

  private apiUrl = 'http://localhost:5057/api/StockMovement';

  getAll(filters?: StockMovementFilters): Observable<StockMovement[]> {

    let params = new HttpParams();

    if (filters?.productId) {
      params = params.set('productId', filters.productId);
    }

    if (filters?.warehouseId) {
      params = params.set('warehouseId', filters.warehouseId);
    }

    if (filters?.movementType) {
      params = params.set('movementType', filters.movementType);
    }

    if (filters?.from) {
      params = params.set('from', filters.from);
    }

    if (filters?.to) {
      params = params.set('to', filters.to);
    }

    return this.http.get<StockMovement[]>(this.apiUrl, { params });
  }

  getReport(): Observable<StockMovementReport> {
    return this.http.get<StockMovementReport>(
      `${this.apiUrl}/report`
    );
  }
}
