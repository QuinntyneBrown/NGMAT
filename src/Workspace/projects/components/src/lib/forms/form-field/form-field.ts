import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'g-form-field',
  standalone: true,
  imports: [CommonModule, MatFormFieldModule],
  templateUrl: './form-field.html',
  styleUrls: ['./form-field.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-form-field]': 'true',
    '[class.g-form-field--required]': 'required',
    '[class.g-form-field--error]': '!!error',
  },
})
export class FormField {
  @Input() label = '';
  @Input() hint = '';
  @Input() error = '';
  @Input() required = false;
}
