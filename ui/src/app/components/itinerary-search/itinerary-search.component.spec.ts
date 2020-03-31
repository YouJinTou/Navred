import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ItinerarySearchComponent } from './itinerary-search.component';

describe('ItinerarySearchComponent', () => {
  let component: ItinerarySearchComponent;
  let fixture: ComponentFixture<ItinerarySearchComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ItinerarySearchComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ItinerarySearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
