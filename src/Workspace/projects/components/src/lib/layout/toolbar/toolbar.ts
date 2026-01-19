import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'g-toolbar',
  standalone: true,
  imports: [CommonModule, MatToolbarModule, MatButtonModule, MatIconModule],
  templateUrl: './toolbar.html',
  styleUrls: ['./toolbar.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-toolbar]': 'true',
    '[class.g-toolbar--with-menu]': 'showMenuButton',
    '[class.g-toolbar--with-search]': 'showSearch',
  },
})
export class Toolbar {
  @Input() title = '';
  @Input() showMenuButton = false;
  @Input() showSearch = false;
  @Output() menuClick = new EventEmitter<void>();

  onMenuClick(): void {
    this.menuClick.emit();
  }
}
