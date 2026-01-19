import type { Meta, StoryObj } from '@storybook/angular';
import { ChipList } from './chip-list';

const meta: Meta<ChipList> = {
  title: 'Components/Forms/ChipList',
  component: ChipList,
  tags: ['autodocs'],
  argTypes: {
    chips: {
      control: 'object',
      description: 'Array of chip items',
    },
    selectable: {
      control: 'boolean',
      description: 'Allow chip selection',
    },
    multiple: {
      control: 'boolean',
      description: 'Allow multiple selection',
    },
  },
};

export default meta;
type Story = StoryObj<ChipList>;

export const Default: Story = {
  args: {
    chips: [
      { label: 'LEO', value: 'leo', selected: false },
      { label: 'MEO', value: 'meo', selected: false },
      { label: 'GEO', value: 'geo', selected: false },
      { label: 'HEO', value: 'heo', selected: false },
    ],
    selectable: true,
    multiple: false,
  },
};

export const SingleSelection: Story = {
  args: {
    chips: [
      { label: 'Keplerian', value: 'keplerian', selected: true },
      { label: 'SGP4', value: 'sgp4', selected: false },
      { label: 'Numerical', value: 'numerical', selected: false },
    ],
    selectable: true,
    multiple: false,
  },
};

export const MultipleSelection: Story = {
  args: {
    chips: [
      { label: 'J2', value: 'j2', selected: true },
      { label: 'Drag', value: 'drag', selected: true },
      { label: 'SRP', value: 'srp', selected: false },
      { label: 'Third Body', value: 'third-body', selected: false },
      { label: 'Tidal', value: 'tidal', selected: false },
    ],
    selectable: true,
    multiple: true,
  },
};

export const NonSelectable: Story = {
  args: {
    chips: [
      { label: 'ISS', value: 'iss' },
      { label: 'Hubble', value: 'hubble' },
      { label: 'GOES-16', value: 'goes-16' },
    ],
    selectable: false,
    multiple: false,
  },
};

export const CoordinateFrames: Story = {
  args: {
    chips: [
      { label: 'ECI J2000', value: 'eci', selected: true },
      { label: 'ECEF ITRF', value: 'ecef', selected: false },
      { label: 'LVLH', value: 'lvlh', selected: false },
      { label: 'VNC', value: 'vnc', selected: false },
    ],
    selectable: true,
    multiple: false,
  },
};
