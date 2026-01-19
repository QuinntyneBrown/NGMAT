import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

@Component({
  selector: 'g-toggle',
  standalone: true,
  imports: [CommonModule, MatSlideToggleModule],
  templateUrl: './toggle.html',
  styleUrls: ['./toggle.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-toggle]': 'true',
    '[class.g-toggle--disabled]': 'disabled',
    '[class.g-toggle--checked]': 'checked',
  },
})
export class Toggle {
  @Input() label = '';
  @Input() checked = false;
  @Input() disabled = false;

  @Output() checkedChange = new EventEmitter<boolean>();

  onToggleChange(checked: boolean): void {
    this.checked = checked;
    this.checkedChange.emit(checked);
  }
}
