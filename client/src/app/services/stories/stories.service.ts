import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Story } from '../../models/story.model';
import { PagedResult } from '../../models/paged-result.model';

@Injectable({
  providedIn: 'root',
})
export class StoriesService {
  private readonly baseUrl = '/api/stories';

  constructor(private readonly http: HttpClient) { }

  getNewest(page: number, pageSize: number, query?: string): Observable<PagedResult<Story>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    const trimmed = query?.trim();
    if (trimmed) {
      params = params.set('query', trimmed);
    }

    return this.http.get<PagedResult<Story>>(`${this.baseUrl}/newest`, { params });
  }
}
