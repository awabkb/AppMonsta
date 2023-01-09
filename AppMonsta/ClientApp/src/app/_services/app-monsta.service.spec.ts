import { TestBed } from '@angular/core/testing';

import { AppMonstaService } from './app-monsta.service';

describe('AppMonstaService', () => {
  let service: AppMonstaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AppMonstaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
