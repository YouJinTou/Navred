import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Navred';
  result: any[];

  onResultFound(paths: any[]) {
    this.result = paths;
  }
}
