import { AsyncPipe, CurrencyPipe, NgFor, NgIf } from '@angular/common';
import { Component, inject } from '@angular/core';
import { catchError, map, of, startWith } from 'rxjs';
import { CsvItem } from './csv-item.model';
import { CsvItemsService } from './csv-items.service';

type ViewModel =
  | { state: 'loading' }
  | { state: 'loaded'; items: CsvItem[] }
  | { state: 'error'; message: string };

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [AsyncPipe, CurrencyPipe, NgFor, NgIf],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  private readonly csvItemsService = inject(CsvItemsService);

  readonly vm$ = this.csvItemsService.getItems().pipe(
    map((items): ViewModel => ({ state: 'loaded', items })),
    startWith({ state: 'loading' } satisfies ViewModel),
    catchError((error) =>
      of({
        state: 'error',
        message: error?.error?.message ?? 'CSV data could not be loaded.'
      } satisfies ViewModel)
    )
  );

  trackById(_index: number, item: CsvItem): string {
    return item.id ?? `${item.name}-${item.category}`;
  }
}
