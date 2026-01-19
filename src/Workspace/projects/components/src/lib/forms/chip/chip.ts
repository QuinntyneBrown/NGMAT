import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'g-chip',
  standalone: true,
  imports: [CommonModule, MatChipsModule, MatIconModule],
  templateUrl: './chip.html',
  styleUrls: ['./chip.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-chip]': 'true',
    '[class.g-chip--selected]': 'selected',
    '[class.g-chip--removable]': 'removable',
  },
})
export class Chip {
  @Input() label = '';
  @Input() selected = false;
  @Input() removable = false;

  @Output() removed = new EventEmitter<void>();

  onRemove(): void {
    this.removed.emit();
  }
}
