import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, catchError, of, tap } from 'rxjs';

import { StoriesService } from './stories.service';
import { Story } from './stories.model';
import { PagedResult } from '../../shared/models/paged-result.model';
import { HighlightPipe } from '../../shared/pipes/highlight.pipe';
import { KeyboardShortcutsDirective } from '../../shared/directives/keyboard-shortcuts.directive';

@Component({
  selector: 'app-stories',
  standalone: true,
  imports: [CommonModule, FormsModule, HighlightPipe, KeyboardShortcutsDirective],
  templateUrl: './stories.component.html',
  styleUrl: './stories.component.scss',
})
export class StoriesComponent implements OnInit {
  stories: Story[] = [];

  page = 1;
  pageSize = 20;
  total = 0;

  query = '';
  loading = false;
  error: string | null = null;

  private readonly query$ = new Subject<string>();
  
  @ViewChild('searchInput')
  private searchInput?: ElementRef<HTMLInputElement>;

  constructor(private readonly storiesService: StoriesService) { }

  ngOnInit(): void {
    this.load();

    this.query$
      .pipe(
        debounceTime(600),
        distinctUntilChanged(),
        tap(() => {
          this.page = 1;
          this.load();
        })
      )
      .subscribe();
  }

  onSearchChange(value: string): void {
    this.query$.next(value);
  }

  load(): void {
    this.loading = true;
    this.error = null;

    this.storiesService.getNewest(this.page, this.pageSize, this.query).pipe(
      tap(res => {
        this.total = res.total;
        this.stories = res.items;
        this.loading = false;
      }),
      catchError(err => {
        this.loading = false;
        this.error = 'Failed to load stories.';
        return of({
          page: this.page,
          pageSize: this.pageSize,
          total: 0,
          items: []
        } as PagedResult<Story>);
      })
    ).subscribe();
  }

  prev(): void {
    if (this.page <= 1) return;
    this.page--;
    this.load();
  }

  next(): void {
    if (this.stories.length < this.pageSize) return;
    this.page++;
    this.load();
  }

  onEscape(): void {
    if (this.query) {
      this.query = '';
      this.onSearchChange('');
    }

    this.searchInput?.nativeElement.blur();
  }
}