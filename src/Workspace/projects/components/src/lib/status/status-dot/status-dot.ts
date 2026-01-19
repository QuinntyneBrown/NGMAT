import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type StatusDotStatus = 'success' | 'warning' | 'error' | 'info';

@Component({
  selector: 'g-status-dot',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-dot.html',
  styleUrls: ['./status-dot.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusDot {
  @Input() status: StatusDotStatus = 'info';
  @Input() pulse = false;
}
