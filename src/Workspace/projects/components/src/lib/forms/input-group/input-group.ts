import { ChangeDetectionStrategy, Component, forwardRef, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';

type InputType = 'text' | 'number';

@Component({
  selector: 'g-input-group',
  standalone: true,
  imports: [CommonModule, FormsModule, MatInputModule, MatFormFieldModule],
  templateUrl: './input-group.html',
  styleUrls: ['./input-group.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputGroup),
      multi: true,
    },
  ],
  host: {
    '[class.g-input-group]': 'true',
    '[class.g-input-group--disabled]': 'disabled',
  },
})
export class InputGroup implements ControlValueAccessor {
  @Input() label = '';
  @Input() unit = '';
  @Input() placeholder = '';
  @Input() type: InputType = 'text';

  value: string | number = '';
  disabled = false;

  private onChange: (value: string | number) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: string | number): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string | number) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const newValue = this.type === 'number' ? parseFloat(input.value) || 0 : input.value;
    this.value = newValue;
    this.onChange(newValue);
  }

  onBlur(): void {
    this.onTouched();
  }
}
