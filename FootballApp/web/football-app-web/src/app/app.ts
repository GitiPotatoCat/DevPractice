import { Component, ChangeDetectionStrategy } from '@angular/core';

import { FootballClubsTableComponent } from './components/football-clubs-table/football-clubs-table.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FootballClubsTableComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly title = 'Football Club Manager';
}