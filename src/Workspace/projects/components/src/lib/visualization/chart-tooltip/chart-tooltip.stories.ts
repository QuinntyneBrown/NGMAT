import { Meta, StoryObj } from '@storybook/angular';
import { ChartTooltip, ChartTooltipData } from './chart-tooltip';

const meta: Meta<ChartTooltip> = {
  title: 'Components/Visualization/ChartTooltip',
  component: ChartTooltip,
  tags: ['autodocs'],
  argTypes: {
    data: {
      control: 'object',
    },
    position: {
      control: 'object',
    },
  },
  decorators: [
    (story) => ({
      ...story,
      template: `<div style="position: relative; height: 300px; background: #f5f5f5; padding: 100px;">
        ${story().template || '<g-chart-tooltip [data]="data" [position]="position"></g-chart-tooltip>'}
      </div>`,
    }),
  ],
};

export default meta;
type Story = StoryObj<ChartTooltip>;

const defaultData: ChartTooltipData = {
  title: '2024-01-15 14:30:00 UTC',
  items: [
    { label: 'Altitude', value: '408.2 km', color: '#3f51b5' },
    { label: 'Velocity', value: '7.66 km/s', color: '#f44336' },
  ],
};

export const Default: Story = {
  args: {
    data: defaultData,
    position: { x: 150, y: 120 },
  },
  render: (args) => ({
    props: args,
    template: `<g-chart-tooltip [data]="data" [position]="position"></g-chart-tooltip>`,
  }),
};

export const WithoutTitle: Story = {
  args: {
    data: {
      items: [
        { label: 'X', value: '6778.14 km', color: '#f44336' },
        { label: 'Y', value: '0.00 km', color: '#4caf50' },
        { label: 'Z', value: '0.00 km', color: '#2196f3' },
      ],
    },
    position: { x: 150, y: 120 },
  },
  render: (args) => ({
    props: args,
    template: `<g-chart-tooltip [data]="data" [position]="position"></g-chart-tooltip>`,
  }),
};

export const SingleItem: Story = {
  args: {
    data: {
      title: 'Perigee',
      items: [
        { label: 'Altitude', value: '200 km' },
      ],
    },
    position: { x: 150, y: 120 },
  },
  render: (args) => ({
    props: args,
    template: `<g-chart-tooltip [data]="data" [position]="position"></g-chart-tooltip>`,
  }),
};

export const ManyItems: Story = {
  args: {
    data: {
      title: 'Spacecraft State',
      items: [
        { label: 'Position X', value: '6778.14 km', color: '#f44336' },
        { label: 'Position Y', value: '1234.56 km', color: '#4caf50' },
        { label: 'Position Z', value: '-567.89 km', color: '#2196f3' },
        { label: 'Velocity X', value: '0.12 km/s', color: '#ff9800' },
        { label: 'Velocity Y', value: '7.54 km/s', color: '#9c27b0' },
        { label: 'Velocity Z', value: '-0.03 km/s', color: '#795548' },
      ],
    },
    position: { x: 150, y: 180 },
  },
  render: (args) => ({
    props: args,
    template: `<g-chart-tooltip [data]="data" [position]="position"></g-chart-tooltip>`,
  }),
};
