import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';

export interface ChipItem {
  label: string;
  value: unknown;
  selected?: boolean;
}

@Component({
  selector: 'g-chip-list',
  standalone: true,
  imports: [CommonModule, MatChipsModule],
  templateUrl: './chip-list.html',
  styleUrls: ['./chip-list.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-chip-list]': 'true',
    '[class.g-chip-list--selectable]': 'selectable',
    '[class.g-chip-list--multiple]': 'multiple',
  },
})
export class ChipList {
  @Input() chips: ChipItem[] = [];
  @Input() selectable = true;
  @Input() multiple = false;

  @Output() selectionChange = new EventEmitter<ChipItem[]>();

  onChipClick(chip: ChipItem): void {
    if (!this.selectable) return;

    if (this.multiple) {
      chip.selected = !chip.selected;
    } else {
      this.chips.forEach((c) => (c.selected = c === chip ? !c.selected : false));
    }

    const selected = this.chips.filter((c) => c.selected);
    this.selectionChange.emit(selected);
  }

  trackByValue(_index: number, chip: ChipItem): unknown {
    return chip.value;
  }
}
