import type { Meta, StoryObj } from '@storybook/angular';
import { Toggle } from './toggle';

const meta: Meta<Toggle> = {
  title: 'Components/Forms/Toggle',
  component: Toggle,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Toggle label text',
    },
    checked: {
      control: 'boolean',
      description: 'Checked state',
    },
    disabled: {
      control: 'boolean',
      description: 'Disabled state',
    },
  },
};

export default meta;
type Story = StoryObj<Toggle>;

export const Default: Story = {
  args: {
    label: 'Enable feature',
    checked: false,
    disabled: false,
  },
};

export const Checked: Story = {
  args: {
    label: 'Feature enabled',
    checked: true,
    disabled: false,
  },
};

export const Disabled: Story = {
  args: {
    label: 'Disabled toggle',
    checked: false,
    disabled: true,
  },
};

export const DisabledChecked: Story = {
  args: {
    label: 'Disabled and checked',
    checked: true,
    disabled: true,
  },
};

export const RealTimeMode: Story = {
  args: {
    label: 'Real-time propagation',
    checked: true,
    disabled: false,
  },
};

export const SimulationSettings: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 16px;">
        <g-toggle label="Auto-update trajectory" [checked]="true"></g-toggle>
        <g-toggle label="Show ground track" [checked]="true"></g-toggle>
        <g-toggle label="Display orbit markers" [checked]="false"></g-toggle>
        <g-toggle label="Enable collision detection" [checked]="false" [disabled]="true"></g-toggle>
      </div>
    `,
  }),
};
