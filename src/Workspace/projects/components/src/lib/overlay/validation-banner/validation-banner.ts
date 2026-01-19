import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ValidationBannerStatus } from './validation-banner.types';

@Component({
  selector: 'g-validation-banner',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './validation-banner.html',
  styleUrls: ['./validation-banner.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ValidationBanner {
  @Input() status: ValidationBannerStatus = 'info';
  @Input() message = '';
  @Input() closable = false;

  @Output() closed = new EventEmitter<void>();

  readonly statusIcons: Record<ValidationBannerStatus, string> = {
    success: 'check_circle',
    warning: 'warning',
    error: 'error',
    info: 'info',
  };

  get icon(): string {
    return this.statusIcons[this.status];
  }

  onClose(): void {
    this.closed.emit();
  }
}
