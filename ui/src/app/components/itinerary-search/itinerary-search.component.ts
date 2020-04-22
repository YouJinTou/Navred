import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { itinerariesUrl } from 'src/environments/environment';

@Component({
  selector: 'app-itinerary-search',
  templateUrl: './itinerary-search.component.html',
  styleUrls: ['./itinerary-search.component.css']
})
export class ItinerarySearchComponent implements OnInit {
  @Output() onResultFound = new EventEmitter<any[]>();
  private from: string;
  private to: string;

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
    const result = [
      {
        "legs": [
          {
            "from": {
              "country": "Bulgaria",
              "name": "Гривица",
              "region": "Плевен",
              "municipality": "Плевен",
              "longitude": null,
              "latitude": null
            },
            "fromId": "Bulgaria|Гривица|Плевен|Плевен",
            "to": {
              "country": "Bulgaria",
              "name": "Плевен",
              "region": "Плевен",
              "municipality": "Плевен",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Плевен|Плевен|Плевен",
            "utcArrival": "2020-04-24T03:53:00",
            "utcDeparture": "2020-04-24T03:42:00",
            "duration": "00:11:00",
            "carrier": "Игнатов транс ЕООД",
            "mode": 0,
            "info": null,
            "price": null,
            "fromSpecific": null,
            "toSpecific": null,
            "departureEstimated": false,
            "arrivalEstimated": false,
            "priceEstimated": false
          },
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
              "name": "Велико Търново",
              "region": "Велико Търново",
              "municipality": "Велико Търново",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Велико Търново|Велико Търново|Велико Търново",
            "utcArrival": "2020-04-24T07:30:00",
            "utcDeparture": "2020-04-24T06:01:00",
            "duration": "01:29:00",
            "carrier": "Турист Сервиз",
            "mode": 0,
            "info": null,
            "price": null,
            "fromSpecific": null,
            "toSpecific": null,
            "departureEstimated": false,
            "arrivalEstimated": false,
            "priceEstimated": false
          },
          {
            "from": {
              "country": "Bulgaria",
              "name": "Велико Търново",
              "region": "Велико Търново",
              "municipality": "Велико Търново",
              "longitude": null,
              "latitude": null
            },
            "fromId": "Bulgaria|Велико Търново|Велико Търново|Велико Търново",
            "to": {
              "country": "Bulgaria",
              "name": "Омуртаг",
              "region": "Търговище",
              "municipality": "Омуртаг",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Омуртаг|Търговище|Омуртаг",
            "utcArrival": "2020-04-24T09:30:00",
            "utcDeparture": "2020-04-24T08:26:00",
            "duration": "01:04:00",
            "carrier": "Етап-Адресс / Груп Плюс",
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
          "duration": "23:57:00",
          "price": null
        }
      },
      {
        "legs": [
          {
            "from": {
              "country": "Bulgaria",
              "name": "Гривица",
              "region": "Плевен",
              "municipality": "Плевен",
              "longitude": null,
              "latitude": null
            },
            "fromId": "Bulgaria|Гривица|Плевен|Плевен",
            "to": {
              "country": "Bulgaria",
              "name": "Плевен",
              "region": "Плевен",
              "municipality": "Плевен",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Плевен|Плевен|Плевен",
            "utcArrival": "2020-04-24T03:53:00",
            "utcDeparture": "2020-04-24T03:42:00",
            "duration": "00:11:00",
            "carrier": "Игнатов транс ЕООД",
            "mode": 0,
            "info": null,
            "price": null,
            "fromSpecific": null,
            "toSpecific": null,
            "departureEstimated": false,
            "arrivalEstimated": false,
            "priceEstimated": false
          },
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
              "name": "Велико Търново",
              "region": "Велико Търново",
              "municipality": "Велико Търново",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Велико Търново|Велико Търново|Велико Търново",
            "utcArrival": "2020-04-24T07:30:00",
            "utcDeparture": "2020-04-24T06:01:00",
            "duration": "01:29:00",
            "carrier": "Турист Сервиз",
            "mode": 0,
            "info": null,
            "price": null,
            "fromSpecific": null,
            "toSpecific": null,
            "departureEstimated": false,
            "arrivalEstimated": false,
            "priceEstimated": false
          },
          {
            "from": {
              "country": "Bulgaria",
              "name": "Велико Търново",
              "region": "Велико Търново",
              "municipality": "Велико Търново",
              "longitude": null,
              "latitude": null
            },
            "fromId": "Bulgaria|Велико Търново|Велико Търново|Велико Търново",
            "to": {
              "country": "Bulgaria",
              "name": "Омуртаг",
              "region": "Търговище",
              "municipality": "Омуртаг",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Омуртаг|Търговище|Омуртаг",
            "utcArrival": "2020-04-24T10:45:00",
            "utcDeparture": "2020-04-24T09:41:00",
            "duration": "01:04:00",
            "carrier": "Юнион Ивкони",
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
          "duration": "1.02:27:00",
          "price": null
        }
      },
      {
        "legs": [
          {
            "from": {
              "country": "Bulgaria",
              "name": "Гривица",
              "region": "Плевен",
              "municipality": "Плевен",
              "longitude": null,
              "latitude": null
            },
            "fromId": "Bulgaria|Гривица|Плевен|Плевен",
            "to": {
              "country": "Bulgaria",
              "name": "Плевен",
              "region": "Плевен",
              "municipality": "Плевен",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Плевен|Плевен|Плевен",
            "utcArrival": "2020-04-24T03:53:00",
            "utcDeparture": "2020-04-24T03:42:00",
            "duration": "00:11:00",
            "carrier": "Игнатов транс ЕООД",
            "mode": 0,
            "info": null,
            "price": null,
            "fromSpecific": null,
            "toSpecific": null,
            "departureEstimated": false,
            "arrivalEstimated": false,
            "priceEstimated": false
          },
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
              "name": "Велико Търново",
              "region": "Велико Търново",
              "municipality": "Велико Търново",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Велико Търново|Велико Търново|Велико Търново",
            "utcArrival": "2020-04-24T07:30:00",
            "utcDeparture": "2020-04-24T06:01:00",
            "duration": "01:29:00",
            "carrier": "Турист Сервиз",
            "mode": 0,
            "info": null,
            "price": null,
            "fromSpecific": null,
            "toSpecific": null,
            "departureEstimated": false,
            "arrivalEstimated": false,
            "priceEstimated": false
          },
          {
            "from": {
              "country": "Bulgaria",
              "name": "Велико Търново",
              "region": "Велико Търново",
              "municipality": "Велико Търново",
              "longitude": null,
              "latitude": null
            },
            "fromId": "Bulgaria|Велико Търново|Велико Търново|Велико Търново",
            "to": {
              "country": "Bulgaria",
              "name": "Омуртаг",
              "region": "Търговище",
              "municipality": "Омуртаг",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Омуртаг|Търговище|Омуртаг",
            "utcArrival": "2020-04-24T12:00:00",
            "utcDeparture": "2020-04-24T10:56:00",
            "duration": "01:04:00",
            "carrier": "Етап-Адресс / Груп Плюс",
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
          "duration": "1.04:57:00",
          "price": null
        }
      },
      {
        "legs": [
          {
            "from": {
              "country": "Bulgaria",
              "name": "Гривица",
              "region": "Плевен",
              "municipality": "Плевен",
              "longitude": null,
              "latitude": null
            },
            "fromId": "Bulgaria|Гривица|Плевен|Плевен",
            "to": {
              "country": "Bulgaria",
              "name": "Плевен",
              "region": "Плевен",
              "municipality": "Плевен",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Плевен|Плевен|Плевен",
            "utcArrival": "2020-04-24T03:53:00",
            "utcDeparture": "2020-04-24T03:42:00",
            "duration": "00:11:00",
            "carrier": "Игнатов транс ЕООД",
            "mode": 0,
            "info": null,
            "price": null,
            "fromSpecific": null,
            "toSpecific": null,
            "departureEstimated": false,
            "arrivalEstimated": false,
            "priceEstimated": false
          },
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
              "name": "Велико Търново",
              "region": "Велико Търново",
              "municipality": "Велико Търново",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Велико Търново|Велико Търново|Велико Търново",
            "utcArrival": "2020-04-24T07:30:00",
            "utcDeparture": "2020-04-24T06:01:00",
            "duration": "01:29:00",
            "carrier": "Турист Сервиз",
            "mode": 0,
            "info": null,
            "price": null,
            "fromSpecific": null,
            "toSpecific": null,
            "departureEstimated": false,
            "arrivalEstimated": false,
            "priceEstimated": false
          },
          {
            "from": {
              "country": "Bulgaria",
              "name": "Велико Търново",
              "region": "Велико Търново",
              "municipality": "Велико Търново",
              "longitude": null,
              "latitude": null
            },
            "fromId": "Bulgaria|Велико Търново|Велико Търново|Велико Търново",
            "to": {
              "country": "Bulgaria",
              "name": "Омуртаг",
              "region": "Търговище",
              "municipality": "Омуртаг",
              "longitude": null,
              "latitude": null
            },
            "toId": "Bulgaria|Омуртаг|Търговище|Омуртаг",
            "utcArrival": "2020-04-24T13:00:00",
            "utcDeparture": "2020-04-24T11:56:00",
            "duration": "01:04:00",
            "carrier": "Етап-Адресс / Груп Плюс",
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
          "duration": "1.06:57:00",
          "price": null
        }
      }
    ]

    this.onResultFound.emit(result);
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
    const url = itinerariesUrl(this.from, this.to, '2020-04-24T04:00:00', '2020-04-24T23:59:00');

    console.log(url);

    this.httpClient.get<any[]>(url).subscribe(r => {
      console.log(r);
      this.onResultFound.emit(r);
    });
  }
}
