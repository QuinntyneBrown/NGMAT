import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type StatusBadgeStatus = 'success' | 'warning' | 'error' | 'info';

@Component({
  selector: 'g-status-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-badge.html',
  styleUrls: ['./status-badge.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadge {
  @Input() label = '';
  @Input() status: StatusBadgeStatus = 'info';
}
