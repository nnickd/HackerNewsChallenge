import { TestBed } from '@angular/core/testing';

import { Stories } from './stories';

describe('Stories', () => {
  let service: Stories;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Stories);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
