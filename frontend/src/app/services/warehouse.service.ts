
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  Warehouse,
  CreateWarehouse,
  UpdateWarehouse
} from '../models/warehouse.model';

@Injectable({
  providedIn: 'root'
})
export class WarehouseService {
  private http = inject(HttpClient);

private apiUrl = 'http://localhost:5057/api/Warehouse';

  getAll(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(this.apiUrl);
  }

  getById(id: number): Observable<Warehouse> {
    return this.http.get<Warehouse>(`${this.apiUrl}/${id}`);
  }

  create(warehouse: CreateWarehouse): Observable<Warehouse> {
    return this.http.post<Warehouse>(
      this.apiUrl,
      warehouse
    );
  }

  update(id: number, warehouse: UpdateWarehouse): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${id}`,
      warehouse
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${id}`
    );
  }
}
