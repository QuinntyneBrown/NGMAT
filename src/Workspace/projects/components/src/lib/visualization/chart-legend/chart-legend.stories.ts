import { Meta, StoryObj } from '@storybook/angular';
import { ChartLegend, ChartLegendItem } from './chart-legend';

const meta: Meta<ChartLegend> = {
  title: 'Components/Visualization/ChartLegend',
  component: ChartLegend,
  tags: ['autodocs'],
  argTypes: {
    items: {
      control: 'object',
    },
  },
};

export default meta;
type Story = StoryObj<ChartLegend>;

const defaultItems: ChartLegendItem[] = [
  { label: 'Altitude', color: '#3f51b5' },
  { label: 'Velocity', color: '#f44336' },
  { label: 'Acceleration', color: '#4caf50' },
];

export const Default: Story = {
  args: {
    items: defaultItems,
  },
};

export const WithValues: Story = {
  args: {
    items: [
      { label: 'Perigee', color: '#2196f3', value: '200 km' },
      { label: 'Apogee', color: '#ff9800', value: '35,786 km' },
      { label: 'Inclination', color: '#9c27b0', value: '0.1°' },
    ],
  },
};

export const SingleItem: Story = {
  args: {
    items: [
      { label: 'Orbital Period', color: '#00bcd4', value: '23h 56m' },
    ],
  },
};

export const ManyItems: Story = {
  args: {
    items: [
      { label: 'Position X', color: '#f44336' },
      { label: 'Position Y', color: '#4caf50' },
      { label: 'Position Z', color: '#2196f3' },
      { label: 'Velocity X', color: '#ff9800' },
      { label: 'Velocity Y', color: '#9c27b0' },
      { label: 'Velocity Z', color: '#795548' },
    ],
  },
};
