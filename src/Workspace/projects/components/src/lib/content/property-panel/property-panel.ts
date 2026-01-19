import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

export interface Property {
  label: string;
  value: string | number;
  unit?: string;
}

@Component({
  selector: 'g-property-panel',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  templateUrl: './property-panel.html',
  styleUrls: ['./property-panel.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PropertyPanel {
  @Input() title = '';
  @Input() properties: Property[] = [];
}
