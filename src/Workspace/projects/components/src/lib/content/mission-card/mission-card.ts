import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

export type MissionStatus = 'planning' | 'active' | 'completed' | 'failed' | 'on-hold';

export interface Mission {
  name: string;
  status: MissionStatus;
  description: string;
  launchDate: Date | string;
}

@Component({
  selector: 'g-mission-card',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './mission-card.html',
  styleUrls: ['./mission-card.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MissionCard {
  @Input() mission: Mission | null = null;

  get statusIcon(): string {
    if (!this.mission) return '';
    switch (this.mission.status) {
      case 'planning':
        return 'edit_calendar';
      case 'active':
        return 'rocket_launch';
      case 'completed':
        return 'check_circle';
      case 'failed':
        return 'error';
      case 'on-hold':
        return 'pause_circle';
      default:
        return 'help';
    }
  }

  get formattedDate(): string {
    if (!this.mission?.launchDate) return '';
    const date = new Date(this.mission.launchDate);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }
}
