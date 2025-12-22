import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, catchError, of, tap } from 'rxjs';

import { StoriesService } from '../../services/stories/stories.service';
import { Story } from '../../models/story.model';
import { PagedResult } from '../../models/paged-result.model';

@Component({
  selector: 'app-stories',
  standalone: true,
  imports: [CommonModule, FormsModule],
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

  constructor(private readonly storiesService: StoriesService) { }

  ngOnInit(): void {
    this.load();

    this.query$
      .pipe(
        debounceTime(350),
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
}