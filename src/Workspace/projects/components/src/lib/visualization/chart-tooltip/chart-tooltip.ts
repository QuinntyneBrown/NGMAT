import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ChartTooltipItem {
  label: string;
  value: string | number;
  color?: string;
}

export interface ChartTooltipData {
  title?: string;
  items: ChartTooltipItem[];
}

export interface ChartTooltipPosition {
  x: number;
  y: number;
}

@Component({
  selector: 'g-chart-tooltip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chart-tooltip.html',
  styleUrls: ['./chart-tooltip.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartTooltip {
  @Input() data: ChartTooltipData | null = null;
  @Input() position: ChartTooltipPosition = { x: 0, y: 0 };
}
