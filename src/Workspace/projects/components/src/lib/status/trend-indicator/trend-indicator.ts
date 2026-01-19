import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

export type TrendDirection = 'up' | 'down' | 'neutral';

@Component({
  selector: 'g-trend-indicator',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './trend-indicator.html',
  styleUrls: ['./trend-indicator.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrendIndicator {
  @Input() value: number | string = 0;
  @Input() direction: TrendDirection = 'neutral';

  get iconName(): string {
    switch (this.direction) {
      case 'up':
        return 'trending_up';
      case 'down':
        return 'trending_down';
      default:
        return 'trending_flat';
    }
  }
}
