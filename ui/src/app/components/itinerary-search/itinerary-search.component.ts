import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { itinerariesUrl } from 'src/environments/environment';

@Component({
  selector: 'app-itinerary-search',
  templateUrl: './itinerary-search.component.html',
  styleUrls: ['./itinerary-search.component.css']
})
export class ItinerarySearchComponent implements OnInit {
  paths: any[];
  @Output() onResultFound = new EventEmitter<any[]>();
  private from: string;
  private to: string;
  private result: any;

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
    this.result = [
      {
          "legs": [
              {
                  "from": {
                      "country": "Bulgaria",
                      "name": "Плевен",
                      "region": "Плевен",
                      "municipality": "Плевен",
                      "longitude": null,
                      "latitude": null
                  },
                  "fromId": "Bulgaria|Плевен|Плевен|Плевен",
                  "to": {
                      "country": "Bulgaria",
                      "name": "Гривица",
                      "region": "Плевен",
                      "municipality": "Плевен",
                      "longitude": null,
                      "latitude": null
                  },
                  "toId": "Bulgaria|Гривица|Плевен|Плевен",
                  "utcArrival": "2020-04-22T04:10:00",
                  "utcDeparture": "2020-04-22T04:00:00",
                  "duration": "00:10:00",
                  "carrier": "Игнатов транс ЕООД",
                  "mode": 0,
                  "info": null,
                  "price": null,
                  "fromSpecific": null,
                  "toSpecific": null,
                  "departureEstimated": false,
                  "arrivalEstimated": false,
                  "priceEstimated": false
              }
          ],
          "weight": {
              "duration": "00:10:00",
              "price": null
          }
      },
      {
          "legs": [
              {
                  "from": {
                      "country": "Bulgaria",
                      "name": "Плевен",
                      "region": "Плевен",
                      "municipality": "Плевен",
                      "longitude": null,
                      "latitude": null
                  },
                  "fromId": "Bulgaria|Плевен|Плевен|Плевен",
                  "to": {
                      "country": "Bulgaria",
                      "name": "Гривица",
                      "region": "Плевен",
                      "municipality": "Плевен",
                      "longitude": null,
                      "latitude": null
                  },
                  "toId": "Bulgaria|Гривица|Плевен|Плевен",
                  "utcArrival": "2020-04-22T04:10:00",
                  "utcDeparture": "2020-04-22T04:00:00",
                  "duration": "00:10:00",
                  "carrier": "Игнатов транс ЕООД",
                  "mode": 0,
                  "info": null,
                  "price": null,
                  "fromSpecific": null,
                  "toSpecific": null,
                  "departureEstimated": false,
                  "arrivalEstimated": false,
                  "priceEstimated": false
              }
          ],
          "weight": {
              "duration": "00:10:00",
              "price": null
          }
      }
  ];
  this.findItineraries();
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
    const url = itinerariesUrl(this.from, this.to, '2020-04-22T04:00:00', '2020-04-22T23:59:00');

    console.log(url);

    this.paths = this.result;

    this.onResultFound.emit(this.paths);
    // this.httpClient.get<any[]>(url).subscribe(r => {
    //   console.log(r);

    //   this.paths = r;
    // });
  }
}
