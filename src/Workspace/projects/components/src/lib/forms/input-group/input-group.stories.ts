import type { Meta, StoryObj } from '@storybook/angular';
import { InputGroup } from './input-group';

const meta: Meta<InputGroup> = {
  title: 'Components/Forms/InputGroup',
  component: InputGroup,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Input label',
    },
    unit: {
      control: 'text',
      description: 'Unit suffix displayed after input',
    },
    placeholder: {
      control: 'text',
      description: 'Placeholder text',
    },
    type: {
      control: { type: 'select' },
      options: ['text', 'number'],
      description: 'Input type',
    },
  },
};

export default meta;
type Story = StoryObj<InputGroup>;

export const Default: Story = {
  args: {
    label: 'Distance',
    unit: 'km',
    placeholder: 'Enter distance',
    type: 'number',
  },
};

export const TextInput: Story = {
  args: {
    label: 'Satellite Name',
    unit: '',
    placeholder: 'Enter satellite name',
    type: 'text',
  },
};

export const WithUnit: Story = {
  args: {
    label: 'Velocity',
    unit: 'm/s',
    placeholder: '0.0',
    type: 'number',
  },
};

export const AltitudeInput: Story = {
  args: {
    label: 'Orbital Altitude',
    unit: 'km',
    placeholder: 'Enter altitude',
    type: 'number',
  },
};

export const AngleInput: Story = {
  args: {
    label: 'Inclination',
    unit: 'deg',
    placeholder: '0.0',
    type: 'number',
  },
};

export const WithoutLabel: Story = {
  args: {
    label: '',
    unit: 's',
    placeholder: 'Time value',
    type: 'number',
  },
};
