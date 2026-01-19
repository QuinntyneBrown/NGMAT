import type { Meta, StoryObj } from '@storybook/angular';
import { Chip } from './chip';

const meta: Meta<Chip> = {
  title: 'Components/Forms/Chip',
  component: Chip,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Chip label text',
    },
    selected: {
      control: 'boolean',
      description: 'Selected state',
    },
    removable: {
      control: 'boolean',
      description: 'Show remove button',
    },
  },
};

export default meta;
type Story = StoryObj<Chip>;

export const Default: Story = {
  args: {
    label: 'Satellite',
    selected: false,
    removable: false,
  },
};

export const Selected: Story = {
  args: {
    label: 'Active',
    selected: true,
    removable: false,
  },
};

export const Removable: Story = {
  args: {
    label: 'ISS',
    selected: false,
    removable: true,
  },
};

export const SelectedRemovable: Story = {
  args: {
    label: 'Hubble',
    selected: true,
    removable: true,
  },
};

export const SatelliteTypes: Story = {
  render: () => ({
    template: `
      <div style="display: flex; gap: 8px; flex-wrap: wrap;">
        <g-chip label="LEO" [selected]="true"></g-chip>
        <g-chip label="MEO"></g-chip>
        <g-chip label="GEO"></g-chip>
        <g-chip label="HEO"></g-chip>
      </div>
    `,
  }),
};

export const SelectedSatellites: Story = {
  render: () => ({
    template: `
      <div style="display: flex; gap: 8px; flex-wrap: wrap;">
        <g-chip label="ISS" [removable]="true"></g-chip>
        <g-chip label="Hubble" [removable]="true"></g-chip>
        <g-chip label="GOES-16" [removable]="true"></g-chip>
      </div>
    `,
  }),
};
