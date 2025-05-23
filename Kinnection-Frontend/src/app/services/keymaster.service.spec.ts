import { TestBed } from '@angular/core/testing';

import { KeymasterService } from './keymaster.service';

describe('KeymasterService', () => {
  let service: KeymasterService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(KeymasterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
