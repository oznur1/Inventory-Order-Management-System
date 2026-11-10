import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { StockMovementReport } from '../models/stock-movement.model';

@Injectable({
  providedIn: 'root'
})
export class StockMovementService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5057/api/StockMovement';

  getReport(): Observable<StockMovementReport> {
    return this.http.get<StockMovementReport>(`${this.apiUrl}/report`);
  }
}