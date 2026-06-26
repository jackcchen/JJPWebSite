import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CsvItem } from './csv-item.model';

@Injectable({ providedIn: 'root' })
export class CsvItemsService {
  private readonly http = inject(HttpClient);

  getItems(): Observable<CsvItem[]> {
    return this.http.get<CsvItem[]>('/api/csv-items');
  }
}
