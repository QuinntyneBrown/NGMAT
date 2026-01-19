import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'g-nav-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './nav-section.html',
  styleUrls: ['./nav-section.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-nav-section]': 'true',
  },
})
export class NavSection {
  @Input() title = '';
}
