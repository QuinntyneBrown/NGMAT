import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'g-notification-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-badge.html',
  styleUrls: ['./notification-badge.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationBadge {
  @Input() count = 0;
  @Input() max = 99;
  @Input() dot = false;

  get displayValue(): string {
    if (this.dot) {
      return '';
    }
    if (this.count > this.max) {
      return `${this.max}+`;
    }
    return String(this.count);
  }

  get isVisible(): boolean {
    return this.dot || this.count > 0;
  }
}
