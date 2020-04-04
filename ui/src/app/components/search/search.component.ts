import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { FormControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { placesUrl } from 'src/environments/environment';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit {
  @Output() onPlaceSelected = new EventEmitter<string>();
  searchControl = new FormControl();
  options: any[] = [];

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
    this.searchControl.valueChanges.subscribe(v => {
      this.options = [];
      this.httpClient.get<any[]>(placesUrl('Bulgaria', v)).subscribe(places => {
        for (var p of places) {
          this.options.push({
            text: `${p.place.name} (${p.place.region}, ${p.place.municipality})`,
            value: p.id
          });
        }
      });
    });
  }

  onSelect(place: string) {
    this.onPlaceSelected.emit(place);
  }

  displayWith(value: string) {
    if (!value) {
      return;
    }
    
    const displayValue = value.split('|')[1];

    return displayValue;
  }
}
