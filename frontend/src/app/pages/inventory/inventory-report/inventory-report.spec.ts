import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InventoryReport } from './inventory-report';

describe('InventoryReport', () => {
  let component: InventoryReport;
  let fixture: ComponentFixture<InventoryReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventoryReport],
    }).compileComponents();

    fixture = TestBed.createComponent(InventoryReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
