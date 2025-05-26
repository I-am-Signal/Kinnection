import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserdashComponent } from './userdash.component';

describe('UserdashComponent', () => {
  let component: UserdashComponent;
  let fixture: ComponentFixture<UserdashComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserdashComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserdashComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
