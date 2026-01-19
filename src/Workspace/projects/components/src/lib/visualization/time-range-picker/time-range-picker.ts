import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';

export interface TimeRangePreset {
  label: string;
  value: string;
  from: Date;
  to: Date;
}

export interface TimeRange {
  from: Date;
  to: Date;
}

@Component({
  selector: 'g-time-range-picker',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatNativeDateModule,
  ],
  templateUrl: './time-range-picker.html',
  styleUrls: ['./time-range-picker.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TimeRangePicker {
  @Input() from: Date = new Date();
  @Input() to: Date = new Date();
  @Input() presets: TimeRangePreset[] = [];

  @Output() rangeChange = new EventEmitter<TimeRange>();

  onFromChange(date: Date): void {
    this.from = date;
    this.emitChange();
  }

  onToChange(date: Date): void {
    this.to = date;
    this.emitChange();
  }

  selectPreset(preset: TimeRangePreset): void {
    this.from = preset.from;
    this.to = preset.to;
    this.emitChange();
  }

  private emitChange(): void {
    this.rangeChange.emit({ from: this.from, to: this.to });
  }
}
