import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatRippleModule } from '@angular/material/core';

@Component({
  selector: 'g-nav-item',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatBadgeModule, MatRippleModule],
  templateUrl: './nav-item.html',
  styleUrls: ['./nav-item.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-nav-item]': 'true',
    '[class.g-nav-item--active]': 'active',
    '[class.g-nav-item--disabled]': 'disabled',
    '[class.g-nav-item--collapsed]': 'collapsed',
  },
})
export class NavItem {
  @Input() label = '';
  @Input() icon = '';
  @Input() active = false;
  @Input() routerLink: string | string[] | null = null;
  @Input() badge: string | number | null = null;
  @Input() disabled = false;
  @Input() collapsed = false;
}
