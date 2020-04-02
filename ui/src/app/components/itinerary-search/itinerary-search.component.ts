import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { itinerariesUrl } from 'src/environments/environment';

@Component({
  selector: 'app-itinerary-search',
  templateUrl: './itinerary-search.component.html',
  styleUrls: ['./itinerary-search.component.css']
})
export class ItinerarySearchComponent implements OnInit {

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
  }

  onSearch() {
    const url = itinerariesUrl('Любимец', 'София', '2020-04-05T06:00:00', '2020-04-05T09:00:00');

    this.httpClient.get(url).subscribe(r => console.log(r));
  }

}
