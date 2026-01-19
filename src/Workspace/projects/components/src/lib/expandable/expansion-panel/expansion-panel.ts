import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';

@Component({
  selector: 'g-expansion-panel',
  standalone: true,
  imports: [CommonModule, MatExpansionModule],
  templateUrl: './expansion-panel.html',
  styleUrls: ['./expansion-panel.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExpansionPanel {
  @Input() title = '';
  @Input() description = '';
  @Input() expanded = false;

  @Output() expandedChange = new EventEmitter<boolean>();

  onExpandedChange(isExpanded: boolean): void {
    this.expanded = isExpanded;
    this.expandedChange.emit(isExpanded);
  }
}
