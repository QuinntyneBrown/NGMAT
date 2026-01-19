import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
  selector: 'g-checkbox',
  standalone: true,
  imports: [CommonModule, MatCheckboxModule],
  templateUrl: './checkbox.html',
  styleUrls: ['./checkbox.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-checkbox]': 'true',
    '[class.g-checkbox--disabled]': 'disabled',
    '[class.g-checkbox--checked]': 'checked',
  },
})
export class Checkbox {
  @Input() label = '';
  @Input() checked = false;
  @Input() disabled = false;

  @Output() checkedChange = new EventEmitter<boolean>();

  onCheckboxChange(checked: boolean): void {
    this.checked = checked;
    this.checkedChange.emit(checked);
  }
}
