import { Component, ElementRef, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
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

  activeIdx = -1;
  itemNav = false;

  private readonly query$ = new Subject<string>();
  
  @ViewChild('searchInput')
  private searchInput?: ElementRef<HTMLInputElement>;
  
  @ViewChildren('storyLink')
  private storyLinks!: QueryList<ElementRef<HTMLAnchorElement>>;

  private _pendingFocus: 'first' | 'last' | null = null;

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
    this.toggleItemNav(false);
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

        if (this._pendingFocus) {
          if (this._pendingFocus == 'first') {
            this.activeIdx = 0;
          } 
          else {
            this.activeIdx = Math.max(0, this.stories.length - 1);
          }

          this._pendingFocus = null;
        }
        
        if (this.itemNav) {
          setTimeout(() => this.focusIdx(this.activeIdx));
        }
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

  prevPage(): void {
    if (this.page <= 1) return;
    this.page--;
    this.load();
  }

  nextPage(): void {
    if (this.stories.length < this.pageSize) return;
    this.page++;
    this.load();
  }

  focusInput(): void {
    this.toggleItemNav(false);
    this.searchInput?.nativeElement.focus();
    this.searchInput?.nativeElement.select();
  }

  onEscape(): void {
    this.toggleItemNav(false, true);
    if (this.query) {
      this.query = '';
      this.onSearchChange('');
    }

    this.searchInput?.nativeElement.blur();
  }

  onPrevItem(): void {
    this.toggleItemNav(true);
    if (this.activeIdx > 0) {
      this.focusIdx(this.activeIdx - 1);
      return;
    }

    if (this.page <= 1 || this.loading) {
      return;
    }
 
    this._pendingFocus = 'last';
    this.page--;
    this.load();
  }

  onNextItem(): void {
    this.toggleItemNav(true);
    const lastIdx = this.stories.length - 1;

    if (this.activeIdx < lastIdx) {
      this.focusIdx(this.activeIdx + 1);
      return;
    }

    if (this.stories.length != this.pageSize || this.loading) return;

    this._pendingFocus = 'first';
    this.page++;
    this.load();
  }

  private focusIdx(idx: number) {
    const links = this.storyLinks.toArray();
    if (!links.length) return;

    const clampedIdx = Math.max(0, Math.min(idx, links.length - 1));
    this.activeIdx = clampedIdx;

    queueMicrotask(() => links[this.activeIdx]?.nativeElement.focus());
  }

  private toggleItemNav(active: boolean, keepActive: boolean = false) {
    this.itemNav = active;
    if (!active && !keepActive) {
      this.activeIdx = -1;
    } 
  }
}