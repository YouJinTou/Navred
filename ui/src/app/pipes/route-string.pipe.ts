import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'routeString'
})
export class RouteStringPipe implements PipeTransform {

  transform(legs: any): any {
    console.log(legs)
    let last = legs.slice(-1)[0];
    let legsMap = legs.map(l => `${l.from.name} ${l.utcDeparture}`);
    let legString = legsMap.join(' - ');
    let endString = ` - ${last.to.name} ${last.utcArrival}`;
    let result = legString + endString;

    return result;
  }

}
