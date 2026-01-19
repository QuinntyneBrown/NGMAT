import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'g-divider',
  standalone: true,
  imports: [CommonModule, MatDividerModule],
  templateUrl: './divider.html',
  styleUrls: ['./divider.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-divider]': 'true',
    '[class.g-divider--vertical]': 'vertical',
    '[class.g-divider--horizontal]': '!vertical',
  },
})
export class Divider {
  @Input() vertical = false;
}
