import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'g-chart-card',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  templateUrl: './chart-card.html',
  styleUrls: ['./chart-card.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartCard {
  @Input() title = '';
  @Input() subtitle = '';
}
