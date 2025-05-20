import { TestBed } from '@angular/core/testing';

import { NetrunnerService } from './netrunner.service';

describe('NetrunnerService', () => {
  let service: NetrunnerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NetrunnerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
