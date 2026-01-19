import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export type IconButtonSize = 'small' | 'medium' | 'large';

@Component({
  selector: 'g-icon-button',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './icon-button.html',
  styleUrls: ['./icon-button.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconButton {
  @Input() icon = '';
  @Input() color = 'primary';
  @Input() size: IconButtonSize = 'medium';
  @Input() disabled = false;

  @Output() clicked = new EventEmitter<MouseEvent>();

  onClick(event: MouseEvent): void {
    if (!this.disabled) {
      this.clicked.emit(event);
    }
  }
}
