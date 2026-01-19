import { ChangeDetectionStrategy, Component, forwardRef, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface Vector3 {
  x: number;
  y: number;
  z: number;
}

@Component({
  selector: 'g-vector-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vector-input.html',
  styleUrls: ['./vector-input.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => VectorInput),
      multi: true,
    },
  ],
  host: {
    '[class.g-vector-input]': 'true',
    '[class.g-vector-input--disabled]': 'disabled',
  },
})
export class VectorInput implements ControlValueAccessor {
  @Input() label = '';
  @Input() unit = '';

  value: Vector3 = { x: 0, y: 0, z: 0 };
  disabled = false;

  private onChange: (value: Vector3) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: Vector3): void {
    this.value = value ?? { x: 0, y: 0, z: 0 };
  }

  registerOnChange(fn: (value: Vector3) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onComponentChange(component: 'x' | 'y' | 'z', event: Event): void {
    const input = event.target as HTMLInputElement;
    const numValue = parseFloat(input.value) || 0;
    this.value = { ...this.value, [component]: numValue };
    this.onChange(this.value);
  }

  onBlur(): void {
    this.onTouched();
  }
}
