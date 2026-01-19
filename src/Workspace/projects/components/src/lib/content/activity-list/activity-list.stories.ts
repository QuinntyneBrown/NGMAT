import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ActivityList, ActivityItem } from './activity-list';

const meta: Meta<ActivityList> = {
  title: 'Components/Content/ActivityList',
  component: ActivityList,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatCardModule, MatIconModule],
    }),
  ],
  argTypes: {
    items: {
      control: 'object',
    },
  },
};

export default meta;
type Story = StoryObj<ActivityList>;

const now = new Date();
const fiveMinutesAgo = new Date(now.getTime() - 5 * 60000);
const twoHoursAgo = new Date(now.getTime() - 2 * 3600000);
const oneDayAgo = new Date(now.getTime() - 24 * 3600000);
const threeDaysAgo = new Date(now.getTime() - 3 * 24 * 3600000);

export const Default: Story = {
  args: {
    items: [
      {
        icon: 'rocket_launch',
        title: 'Mission launched successfully',
        description: 'Artemis III launched from Kennedy Space Center LC-39A with nominal trajectory.',
        timestamp: fiveMinutesAgo,
      },
      {
        icon: 'check_circle',
        title: 'Pre-launch checks completed',
        description: 'All systems verified and ready for launch sequence initiation.',
        timestamp: twoHoursAgo,
      },
      {
        icon: 'local_gas_station',
        title: 'Propellant loading complete',
        description: 'LOX and RP-1 tanks at full capacity. Cryogenic systems nominal.',
        timestamp: oneDayAgo,
      },
      {
        icon: 'engineering',
        title: 'Final vehicle integration',
        description: 'Payload successfully mated with launch vehicle. Electrical connections verified.',
        timestamp: threeDaysAgo,
      },
    ] as ActivityItem[],
  },
};

export const MissionTimeline: Story = {
  args: {
    items: [
      {
        icon: 'satellite_alt',
        title: 'Orbital insertion confirmed',
        description: 'Spacecraft has achieved stable low Earth orbit at 408km altitude.',
        timestamp: new Date(now.getTime() - 30 * 60000),
      },
      {
        icon: 'offline_bolt',
        title: 'Stage separation successful',
        description: 'First and second stages separated cleanly. Second stage ignition nominal.',
        timestamp: new Date(now.getTime() - 35 * 60000),
      },
      {
        icon: 'flight_takeoff',
        title: 'Liftoff',
        description: 'Vehicle has cleared the tower. All engines at full thrust.',
        timestamp: new Date(now.getTime() - 40 * 60000),
      },
      {
        icon: 'timer',
        title: 'T-0 countdown initiated',
        description: 'Final automated launch sequence has begun.',
        timestamp: new Date(now.getTime() - 45 * 60000),
      },
    ] as ActivityItem[],
  },
};

export const SystemAlerts: Story = {
  args: {
    items: [
      {
        icon: 'warning',
        title: 'Thermal anomaly detected',
        description: 'Sensor array 3 reporting elevated temperatures. Investigating.',
        timestamp: new Date(now.getTime() - 15 * 60000),
      },
      {
        icon: 'update',
        title: 'Software update deployed',
        description: 'Navigation system firmware v2.4.1 successfully installed.',
        timestamp: new Date(now.getTime() - 4 * 3600000),
      },
      {
        icon: 'backup',
        title: 'Telemetry backup completed',
        description: 'All mission data archived to secondary storage systems.',
        timestamp: new Date(now.getTime() - 12 * 3600000),
      },
    ] as ActivityItem[],
  },
};

export const InCard: Story = {
  render: () => ({
    template: `
      <mat-card style="padding: 20px; max-width: 500px;">
        <h3 style="margin: 0 0 16px 0;">Recent Activity</h3>
        <g-activity-list
          [items]="[
            {
              icon: 'person_add',
              title: 'New crew member assigned',
              description: 'Dr. Sarah Chen assigned to mission as payload specialist.',
              timestamp: '${fiveMinutesAgo.toISOString()}'
            },
            {
              icon: 'edit',
              title: 'Mission plan updated',
              description: 'EVA schedule revised to accommodate weather window.',
              timestamp: '${twoHoursAgo.toISOString()}'
            },
            {
              icon: 'inventory',
              title: 'Cargo manifest finalized',
              description: 'All supplies and equipment logged and secured.',
              timestamp: '${oneDayAgo.toISOString()}'
            }
          ]"
        ></g-activity-list>
      </mat-card>
    `,
  }),
};

export const SingleItem: Story = {
  args: {
    items: [
      {
        icon: 'notifications',
        title: 'System notification',
        description: 'Ground station handover completed. Now tracking via Madrid DSN.',
        timestamp: now,
      },
    ] as ActivityItem[],
  },
};
