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
  options: string[] = [];

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
    this.searchControl.valueChanges.subscribe(v => {
      this.httpClient.get<any[]>(placesUrl('Bulgaria', v)).subscribe(places => {
        this.options = places.map(p => `${p.name} (${p.region}, ${p.municipality})`);
      });
    });
  }

  onSelect(place: string) {
    this.onPlaceSelected.emit(place);
  }
}
