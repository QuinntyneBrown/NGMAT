import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

export interface InfoItem {
  icon: string;
  label: string;
  value: string | number;
}

@Component({
  selector: 'g-info-panel',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './info-panel.html',
  styleUrls: ['./info-panel.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InfoPanel {
  @Input() title = '';
  @Input() items: InfoItem[] = [];
}
