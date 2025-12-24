import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { StoriesService } from './stories.service';
import { PagedResult } from '../../shared/models/paged-result.model';
import { Story } from './stories.model';

describe('StoriesService', () => {
  let service: StoriesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), StoriesService],
    });

    service = TestBed.inject(StoriesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call /api/stories/newest with page and pageSize', () => {
    service.getNewest(2, 25).subscribe();

    const req = httpMock.expectOne((r) =>
      r.method === 'GET' &&
      r.url === '/api/stories/newest' &&
      r.params.get('page') === '2' &&
      r.params.get('pageSize') === '25'
    );

    expect(req.request.method).toBe('GET');
    req.flush({ page: 2, pageSize: 25, total: 0, items: [] });
  });

  it('should trim query before sending it', () => {
    service.getNewest(1, 20, '  angular  ').subscribe();

    const req = httpMock.expectOne((r) =>
      r.url === '/api/stories/newest' &&
      r.params.get('query') === 'angular'
    );

    expect(req.request.params.get('query')).toBe('angular');
    req.flush({ page: 1, pageSize: 20, total: 0, items: [] });
  });

  it('should not send query param when query is empty', () => {
    service.getNewest(1, 20, '   ').subscribe();

    const req = httpMock.expectOne((r) =>
      r.url === '/api/stories/newest' && !r.params.has('query')
    );

    expect(req.request.params.has('query')).toBeFalse();
    req.flush({ page: 1, pageSize: 20, total: 0, items: [] });
  });

  it('should return a PagedResult<Story>', (done) => {
    const mockResponse: PagedResult<Story> = {
      page: 1,
      pageSize: 20,
      total: 1,
      items: [
        {
          id: 1,
          title: 'Test Story',
          link: 'https://example.com',
          createdAt: '2025-01-01T00:00:00Z',
          score: 10,
        },
      ],
    };

    service.getNewest(1, 20).subscribe((res) => {
      expect(res.total).toBe(1);
      expect(res.items[0].title).toBe('Test Story');
      done();
    });

    const req = httpMock.expectOne((r) =>
      r.url === '/api/stories/newest' &&
      r.params.get('page') === '1' &&
      r.params.get('pageSize') === '20'
    );

    expect(req.request.url).toBe('/api/stories/newest');
    req.flush(mockResponse);
  });
});
