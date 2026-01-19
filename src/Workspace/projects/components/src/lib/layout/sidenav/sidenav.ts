import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSidenavModule } from '@angular/material/sidenav';

type SidenavMode = 'over' | 'push' | 'side';

@Component({
  selector: 'g-sidenav',
  standalone: true,
  imports: [CommonModule, MatSidenavModule],
  templateUrl: './sidenav.html',
  styleUrls: ['./sidenav.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-sidenav]': 'true',
    '[class.g-sidenav--opened]': 'opened',
    '[class.g-sidenav--collapsed]': 'collapsed',
    '[class.g-sidenav--over]': 'mode === "over"',
    '[class.g-sidenav--push]': 'mode === "push"',
    '[class.g-sidenav--side]': 'mode === "side"',
  },
})
export class Sidenav {
  @Input() opened = false;
  @Input() mode: SidenavMode = 'side';
  @Input() collapsed = false;
}
