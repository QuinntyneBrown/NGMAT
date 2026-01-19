import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ChartLegendItem {
  label: string;
  color: string;
  value?: string | number;
}

@Component({
  selector: 'g-chart-legend',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chart-legend.html',
  styleUrls: ['./chart-legend.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartLegend {
  @Input() items: ChartLegendItem[] = [];
}
