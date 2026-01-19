import type { Meta, StoryObj } from '@storybook/angular';
import { FilterChips } from './filter-chips';

const meta: Meta<FilterChips> = {
  title: 'Components/Forms/FilterChips',
  component: FilterChips,
  tags: ['autodocs'],
  argTypes: {
    options: {
      control: 'object',
      description: 'Array of filter options',
    },
    selected: {
      control: 'object',
      description: 'Array of selected values',
    },
    showAll: {
      control: 'boolean',
      description: 'Show "All" option',
    },
  },
};

export default meta;
type Story = StoryObj<FilterChips>;

export const Default: Story = {
  args: {
    options: [
      { label: 'Active', value: 'active' },
      { label: 'Inactive', value: 'inactive' },
      { label: 'Pending', value: 'pending' },
    ],
    selected: [],
    showAll: true,
  },
};

export const WithSelection: Story = {
  args: {
    options: [
      { label: 'LEO', value: 'leo' },
      { label: 'MEO', value: 'meo' },
      { label: 'GEO', value: 'geo' },
      { label: 'HEO', value: 'heo' },
    ],
    selected: ['leo', 'meo'],
    showAll: true,
  },
};

export const WithoutAllOption: Story = {
  args: {
    options: [
      { label: 'Communication', value: 'comm' },
      { label: 'Navigation', value: 'nav' },
      { label: 'Earth Observation', value: 'eo' },
      { label: 'Scientific', value: 'sci' },
    ],
    selected: ['comm'],
    showAll: false,
  },
};

export const MissionTypes: Story = {
  args: {
    options: [
      { label: 'Reconnaissance', value: 'recon' },
      { label: 'Communications', value: 'comms' },
      { label: 'Navigation', value: 'nav' },
      { label: 'Weather', value: 'weather' },
      { label: 'Research', value: 'research' },
    ],
    selected: [],
    showAll: true,
  },
};

export const PropagatorFilters: Story = {
  args: {
    options: [
      { label: 'Two-Body', value: 'two-body' },
      { label: 'SGP4/SDP4', value: 'sgp4' },
      { label: 'Numerical', value: 'numerical' },
      { label: 'HPOP', value: 'hpop' },
    ],
    selected: ['two-body', 'sgp4'],
    showAll: true,
  },
};

export const StatusFilters: Story = {
  args: {
    options: [
      { label: 'Operational', value: 'operational' },
      { label: 'Decayed', value: 'decayed' },
      { label: 'Unknown', value: 'unknown' },
    ],
    selected: ['operational'],
    showAll: true,
  },
};
