import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Navred';
  paths: any[];

  onResultFound(paths: any[]) {
    this.paths = paths;
  }
}
