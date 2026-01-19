import { ChangeDetectionStrategy, Component, forwardRef, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatNativeDateModule } from '@angular/material/core';

export interface DateTimeValue {
  date: Date | null;
  time: string;
}

@Component({
  selector: 'g-datetime-group',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDatepickerModule,
    MatInputModule,
    MatFormFieldModule,
    MatNativeDateModule,
  ],
  templateUrl: './datetime-group.html',
  styleUrls: ['./datetime-group.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DatetimeGroup),
      multi: true,
    },
  ],
  host: {
    '[class.g-datetime-group]': 'true',
    '[class.g-datetime-group--disabled]': 'disabled',
  },
})
export class DatetimeGroup implements ControlValueAccessor {
  @Input() label = '';

  dateValue: Date | null = null;
  timeValue = '00:00:00';
  disabled = false;

  private onChange: (value: DateTimeValue) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: DateTimeValue): void {
    if (value) {
      this.dateValue = value.date;
      this.timeValue = value.time || '00:00:00';
    } else {
      this.dateValue = null;
      this.timeValue = '00:00:00';
    }
  }

  registerOnChange(fn: (value: DateTimeValue) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onDateChange(date: Date | null): void {
    this.dateValue = date;
    this.emitChange();
  }

  onTimeChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.timeValue = input.value;
    this.emitChange();
  }

  onBlur(): void {
    this.onTouched();
  }

  private emitChange(): void {
    this.onChange({
      date: this.dateValue,
      time: this.timeValue,
    });
  }
}
