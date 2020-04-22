import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'routeString'
})
export class RouteStringPipe implements PipeTransform {

  transform(legs: any): any {
    let datePipe = new DatePipe('en-US');
    let last = legs.slice(-1)[0];
    let legsMap = legs.map(l => `${l.from.name} ${datePipe.transform(l.utcDeparture, 'hh:mm')}`);
    let legString = legsMap.join(' - ');
    let endString = ` - ${last.to.name} ${datePipe.transform(last.utcArrival, 'hh:mm')}`;
    let result = legString + endString;

    return result;
  }
}
