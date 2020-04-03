import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { itinerariesUrl } from 'src/environments/environment';

@Component({
  selector: 'app-itinerary-search',
  templateUrl: './itinerary-search.component.html',
  styleUrls: ['./itinerary-search.component.css']
})
export class ItinerarySearchComponent implements OnInit {
  paths: any[];
  private from: string;
  private to: string;

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
  }

  setFrom(place: string) {
    this.from = place;

    if (this.from && this.to) {
      this.findItineraries();
    }
  }

  setTo(place: string) {
    this.to = place;

    if (this.from && this.to) {
      this.findItineraries();
    }
  }

  private findItineraries() {
    const url = itinerariesUrl(this.from, this.to, '2020-04-05T06:00:00', '2020-04-05T19:30:00');

    console.log(url);

    this.httpClient.get<any[]>(url).subscribe(r => this.paths = r);
  }
}
