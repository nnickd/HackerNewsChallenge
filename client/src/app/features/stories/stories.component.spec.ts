import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { StoriesComponent } from './stories.component';
import { StoriesService } from './stories.service';
import { Story } from './stories.model';
import { PagedResult } from '../../shared/models/paged-result.model';

describe('StoriesComponent', () => {
  let fixture: ComponentFixture<StoriesComponent>;
  let component: StoriesComponent;

  let storiesServiceSpy: jasmine.SpyObj<StoriesService>;

  const makeStory = (id: number): Story => ({
    id,
    title: `Story ${id}`,
    link: `https://example.com/${id}`,
    by: 'tester',
    createdAt: new Date().toISOString(),
    score: 10,
    commentCount: 5,
  });

  const makePage = (items: Story[]): PagedResult<Story> => ({
    page: 1,
    pageSize: 20,
    total: items.length,
    items,
  });

  beforeEach(async () => {
    storiesServiceSpy = jasmine.createSpyObj<StoriesService>('StoriesService', ['getNewest']);

    storiesServiceSpy.getNewest.and.returnValue(of(makePage([makeStory(1), makeStory(2)])));

    await TestBed.configureTestingModule({
      imports: [StoriesComponent],
      providers: [
        { provide: StoriesService, useValue: storiesServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(StoriesComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load stories on init', () => {
    fixture.detectChanges();

    expect(storiesServiceSpy.getNewest).toHaveBeenCalled();
    const args = storiesServiceSpy.getNewest.calls.mostRecent().args;

    expect(args[0]).toBe(1);
    expect(args[1]).toBe(20);
    expect(args[2]).toBe('');
  });

  it('should render stories after load', () => {
    fixture.detectChanges();

    const links = fixture.nativeElement.querySelectorAll('a');
    expect(links.length).toBeGreaterThan(0);
    expect(links[0].textContent).toContain('Story');
  });

  it('should show error text when service fails', () => {
    storiesServiceSpy.getNewest.and.returnValue(throwError(() => new Error('boom')));

    fixture.detectChanges();

    expect(component.error).toBe('Failed to load stories.');

    const errorEl = fixture.nativeElement.querySelector('.error');
    expect(errorEl?.textContent).toContain('Failed to load stories.');
  });

  it('should debounce search and reload from page 1', fakeAsync(() => {
    fixture.detectChanges();

    component.page = 5;
    component.onSearchChange('angular');

    tick(599);
    expect(storiesServiceSpy.getNewest).toHaveBeenCalledTimes(1);

    tick(1);
    expect(component.page).toBe(1);
    expect(storiesServiceSpy.getNewest).toHaveBeenCalledTimes(2);
  }));
});
