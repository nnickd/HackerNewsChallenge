import { TestBed } from '@angular/core/testing';

import { StoriesService } from './stories.service';

describe('Stories', () => {
  let service: StoriesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StoriesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
