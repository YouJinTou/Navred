import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit {
  searchControl = new FormControl();
  options: string[] = ['Варна', 'София', 'Търговище', 'Велико Търново', 'Русе', 'Пловдив'];
  filteredOptions: Observable<string[]>;

  ngOnInit() {
    this.filteredOptions = this.searchControl.valueChanges.pipe(
      startWith(''),
      map(v => this.options.filter(o => o.toLowerCase().includes(v.toLowerCase())))
    );
  }
}
