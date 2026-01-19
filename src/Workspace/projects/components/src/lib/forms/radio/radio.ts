import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatRadioModule } from '@angular/material/radio';

@Component({
  selector: 'g-radio',
  standalone: true,
  imports: [CommonModule, MatRadioModule],
  templateUrl: './radio.html',
  styleUrls: ['./radio.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-radio]': 'true',
    '[class.g-radio--disabled]': 'disabled',
    '[class.g-radio--checked]': 'checked',
  },
})
export class Radio {
  @Input() label = '';
  @Input() value: unknown = '';
  @Input() name = '';
  @Input() checked = false;
  @Input() disabled = false;

  @Output() checkedChange = new EventEmitter<boolean>();

  onRadioChange(): void {
    this.checkedChange.emit(true);
  }
}
