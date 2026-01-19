import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { Router } from '@angular/router';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-shell',
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
  ],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  protected readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Missions', icon: 'rocket_launch', route: '/missions' },
    { label: 'Spacecraft', icon: 'satellite_alt', route: '/spacecraft' },
    { label: 'Orbit Visualization', icon: 'public', route: '/orbit' },
    { label: 'Maneuver Planner', icon: 'swap_vert', route: '/maneuvers' },
    { label: 'Propagation', icon: 'timeline', route: '/propagation' },
    { label: 'Charts', icon: 'analytics', route: '/charts' },
    { label: 'Ground Track', icon: 'map', route: '/ground-track' },
    { label: 'Script Editor', icon: 'code', route: '/scripts' },
    { label: 'Reports', icon: 'assessment', route: '/reports' },
  ];

  protected readonly notificationCount = signal(3);
  protected readonly sidenavOpened = signal(true);

  constructor(private readonly router: Router) {}

  protected logout(): void {
    // TODO: Implement actual logout
    this.router.navigate(['/login']);
  }

  protected toggleSidenav(): void {
    this.sidenavOpened.update((value) => !value);
  }
}
