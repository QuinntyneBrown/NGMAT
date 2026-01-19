import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'g-editor-section',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './editor-section.html',
  styleUrls: ['./editor-section.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditorSection {
  @Input() title = '';
  @Input() icon = '';
  @Input() expanded = true;

  @Output() expandedChange = new EventEmitter<boolean>();

  toggle(): void {
    this.expanded = !this.expanded;
    this.expandedChange.emit(this.expanded);
  }
}
