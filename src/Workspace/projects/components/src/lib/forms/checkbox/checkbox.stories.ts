import type { Meta, StoryObj } from '@storybook/angular';
import { Checkbox } from './checkbox';

const meta: Meta<Checkbox> = {
  title: 'Components/Forms/Checkbox',
  component: Checkbox,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Checkbox label text',
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
type Story = StoryObj<Checkbox>;

export const Default: Story = {
  args: {
    label: 'Enable feature',
    checked: false,
    disabled: false,
  },
};

export const Checked: Story = {
  args: {
    label: 'Active option',
    checked: true,
    disabled: false,
  },
};

export const Disabled: Story = {
  args: {
    label: 'Disabled option',
    checked: false,
    disabled: true,
  },
};

export const DisabledChecked: Story = {
  args: {
    label: 'Disabled checked',
    checked: true,
    disabled: true,
  },
};

export const IncludePerturbations: Story = {
  args: {
    label: 'Include atmospheric drag',
    checked: true,
    disabled: false,
  },
};

export const MultipleOptions: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 8px;">
        <g-checkbox label="Include J2 perturbation" [checked]="true"></g-checkbox>
        <g-checkbox label="Include atmospheric drag" [checked]="true"></g-checkbox>
        <g-checkbox label="Include solar radiation pressure" [checked]="false"></g-checkbox>
        <g-checkbox label="Include third body effects" [checked]="false"></g-checkbox>
      </div>
    `,
  }),
};
