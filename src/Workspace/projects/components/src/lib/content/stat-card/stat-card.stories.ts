import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { StatCard, TrendData } from './stat-card';

const meta: Meta<StatCard> = {
  title: 'Components/Content/StatCard',
  component: StatCard,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatCardModule, MatIconModule],
    }),
  ],
  argTypes: {
    label: {
      control: 'text',
    },
    value: {
      control: 'text',
    },
    icon: {
      control: 'text',
    },
    trend: {
      control: 'object',
    },
  },
};

export default meta;
type Story = StoryObj<StatCard>;

export const Default: Story = {
  args: {
    label: 'Active Satellites',
    value: '1,247',
    icon: 'satellite_alt',
    trend: null,
  },
};

export const WithTrendUp: Story = {
  args: {
    label: 'Launch Success Rate',
    value: '98.5%',
    icon: 'rocket_launch',
    trend: { value: '+2.3%', direction: 'up' } as TrendData,
  },
};

export const WithTrendDown: Story = {
  args: {
    label: 'Fuel Remaining',
    value: '45%',
    icon: 'local_gas_station',
    trend: { value: '-5%', direction: 'down' } as TrendData,
  },
};

export const WithTrendNeutral: Story = {
  args: {
    label: 'Orbital Velocity',
    value: '7.8 km/s',
    icon: 'speed',
    trend: { value: '0%', direction: 'neutral' } as TrendData,
  },
};

export const NoIcon: Story = {
  args: {
    label: 'Total Missions',
    value: '342',
    icon: '',
    trend: { value: '+12', direction: 'up' } as TrendData,
  },
};

export const MultipleCards: Story = {
  render: () => ({
    template: `
      <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px;">
        <g-stat-card
          label="Active Satellites"
          value="1,247"
          icon="satellite_alt"
          [trend]="{ value: '+24', direction: 'up' }"
        ></g-stat-card>
        <g-stat-card
          label="Ground Stations"
          value="48"
          icon="cell_tower"
          [trend]="{ value: '0', direction: 'neutral' }"
        ></g-stat-card>
        <g-stat-card
          label="Signal Strength"
          value="-82 dBm"
          icon="signal_cellular_alt"
          [trend]="{ value: '-3 dBm', direction: 'down' }"
        ></g-stat-card>
      </div>
    `,
  }),
};
