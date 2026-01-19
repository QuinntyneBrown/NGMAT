import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';

export interface FilterOption {
  label: string;
  value: string;
}

@Component({
  selector: 'g-filter-chips',
  standalone: true,
  imports: [CommonModule, MatChipsModule],
  templateUrl: './filter-chips.html',
  styleUrls: ['./filter-chips.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-filter-chips]': 'true',
  },
})
export class FilterChips {
  @Input() options: FilterOption[] = [];
  @Input() selected: string[] = [];
  @Input() showAll = true;

  @Output() selectionChange = new EventEmitter<string[]>();

  get isAllSelected(): boolean {
    return this.selected.length === 0;
  }

  isSelected(value: string): boolean {
    return this.selected.includes(value);
  }

  onAllClick(): void {
    this.selected = [];
    this.selectionChange.emit([]);
  }

  onOptionClick(option: FilterOption): void {
    const index = this.selected.indexOf(option.value);
    if (index === -1) {
      this.selected = [...this.selected, option.value];
    } else {
      this.selected = this.selected.filter((v) => v !== option.value);
    }
    this.selectionChange.emit(this.selected);
  }

  trackByValue(_index: number, option: FilterOption): string {
    return option.value;
  }
}
