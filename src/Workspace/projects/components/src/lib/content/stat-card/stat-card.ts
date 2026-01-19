import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { TrendDirection } from '../../status/trend-indicator/trend-indicator';

export interface TrendData {
  value: number | string;
  direction: TrendDirection;
}

@Component({
  selector: 'g-stat-card',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './stat-card.html',
  styleUrls: ['./stat-card.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatCard {
  @Input() label = '';
  @Input() value: string | number = '';
  @Input() icon = '';
  @Input() trend: TrendData | null = null;

  get trendIcon(): string {
    if (!this.trend) return '';
    switch (this.trend.direction) {
      case 'up':
        return 'trending_up';
      case 'down':
        return 'trending_down';
      default:
        return 'trending_flat';
    }
  }
}
