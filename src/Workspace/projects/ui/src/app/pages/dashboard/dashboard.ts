import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  protected readonly stats = [
    { label: 'Active Missions', value: '12', icon: 'rocket_launch' },
    { label: 'Spacecraft', value: '8', icon: 'satellite_alt' },
    { label: 'Maneuvers Planned', value: '24', icon: 'swap_vert' },
    { label: 'Simulations Run', value: '156', icon: 'analytics' },
  ];
}
