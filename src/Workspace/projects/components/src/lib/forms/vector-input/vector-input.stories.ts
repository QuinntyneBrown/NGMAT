import type { Meta, StoryObj } from '@storybook/angular';
import { VectorInput } from './vector-input';

const meta: Meta<VectorInput> = {
  title: 'Components/Forms/VectorInput',
  component: VectorInput,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Vector input label',
    },
    unit: {
      control: 'text',
      description: 'Unit displayed after label',
    },
  },
};

export default meta;
type Story = StoryObj<VectorInput>;

export const Default: Story = {
  args: {
    label: 'Position Vector',
    unit: 'km',
  },
};

export const Velocity: Story = {
  args: {
    label: 'Velocity Vector',
    unit: 'm/s',
  },
};

export const Acceleration: Story = {
  args: {
    label: 'Acceleration',
    unit: 'm/s\u00B2',
  },
};

export const WithoutUnit: Story = {
  args: {
    label: 'Direction Vector',
    unit: '',
  },
};

export const WithoutLabel: Story = {
  args: {
    label: '',
    unit: '',
  },
};

export const AngularMomentum: Story = {
  args: {
    label: 'Angular Momentum',
    unit: 'kg\u00B7m\u00B2/s',
  },
};
