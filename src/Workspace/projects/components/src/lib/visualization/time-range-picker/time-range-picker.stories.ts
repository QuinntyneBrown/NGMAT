import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TimeRangePicker, TimeRangePreset } from './time-range-picker';

const meta: Meta<TimeRangePicker> = {
  title: 'Components/Visualization/TimeRangePicker',
  component: TimeRangePicker,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [
        BrowserAnimationsModule,
        FormsModule,
        MatButtonModule,
        MatIconModule,
        MatMenuModule,
        MatDatepickerModule,
        MatFormFieldModule,
        MatInputModule,
        MatNativeDateModule,
      ],
    }),
  ],
  argTypes: {
    from: {
      control: 'date',
    },
    to: {
      control: 'date',
    },
    presets: {
      control: 'object',
    },
    rangeChange: { action: 'rangeChange' },
  },
};

export default meta;
type Story = StoryObj<TimeRangePicker>;

const now = new Date();
const yesterday = new Date(now.getTime() - 24 * 60 * 60 * 1000);
const lastWeek = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
const lastMonth = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);

const defaultPresets: TimeRangePreset[] = [
  { label: 'Last 24 hours', value: '24h', from: yesterday, to: now },
  { label: 'Last 7 days', value: '7d', from: lastWeek, to: now },
  { label: 'Last 30 days', value: '30d', from: lastMonth, to: now },
];

export const Default: Story = {
  args: {
    from: lastWeek,
    to: now,
    presets: [],
  },
};

export const WithPresets: Story = {
  args: {
    from: lastWeek,
    to: now,
    presets: defaultPresets,
  },
};

export const MissionPresets: Story = {
  args: {
    from: lastWeek,
    to: now,
    presets: [
      { label: 'Mission Start to Now', value: 'mission', from: new Date('2024-01-01'), to: now },
      { label: 'Last Orbit', value: 'orbit', from: new Date(now.getTime() - 90 * 60 * 1000), to: now },
      { label: 'Last Pass', value: 'pass', from: new Date(now.getTime() - 15 * 60 * 1000), to: now },
      { label: 'Next 24 hours', value: 'next24h', from: now, to: new Date(now.getTime() + 24 * 60 * 60 * 1000) },
    ],
  },
};

export const CustomRange: Story = {
  args: {
    from: new Date('2024-03-01'),
    to: new Date('2024-03-15'),
    presets: [],
  },
};
