import { Component } from '@angular/core';
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
import { BehaviorSubject } from 'rxjs';
import { AuthService } from '../../core';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

interface NavSection {
  title: string;
  items: NavItem[];
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

  protected readonly adminNavItems: NavItem[] = [
    { label: 'Users', icon: 'people', route: '/users' },
    { label: 'Roles', icon: 'admin_panel_settings', route: '/roles' },
    { label: 'Permissions', icon: 'security', route: '/permissions' },
    { label: 'API Keys', icon: 'key', route: '/api-keys' },
  ];

  protected readonly notificationCount$ = new BehaviorSubject<number>(3);
  protected readonly sidenavOpened$ = new BehaviorSubject<boolean>(true);

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService
  ) {}

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  protected toggleSidenav(): void {
    this.sidenavOpened$.next(!this.sidenavOpened$.value);
  }
}
