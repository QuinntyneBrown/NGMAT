import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatTabsModule } from '@angular/material/tabs';
import { ChartTabs, ChartTab } from './chart-tabs';

const meta: Meta<ChartTabs> = {
  title: 'Components/Visualization/ChartTabs',
  component: ChartTabs,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatTabsModule],
    }),
  ],
  argTypes: {
    tabs: {
      control: 'object',
    },
    selectedTab: {
      control: 'text',
    },
    tabChange: { action: 'tabChange' },
  },
};

export default meta;
type Story = StoryObj<ChartTabs>;

const defaultTabs: ChartTab[] = [
  { label: 'Position', value: 'position' },
  { label: 'Velocity', value: 'velocity' },
  { label: 'Acceleration', value: 'acceleration' },
];

export const Default: Story = {
  args: {
    tabs: defaultTabs,
    selectedTab: 'position',
  },
};

export const SecondTabSelected: Story = {
  args: {
    tabs: defaultTabs,
    selectedTab: 'velocity',
  },
};

export const TwoTabs: Story = {
  args: {
    tabs: [
      { label: '2D View', value: '2d' },
      { label: '3D View', value: '3d' },
    ],
    selectedTab: '2d',
  },
};

export const ManyTabs: Story = {
  args: {
    tabs: [
      { label: 'Overview', value: 'overview' },
      { label: 'Orbit', value: 'orbit' },
      { label: 'Ground Track', value: 'ground-track' },
      { label: 'Coverage', value: 'coverage' },
      { label: 'Events', value: 'events' },
    ],
    selectedTab: 'overview',
  },
};
