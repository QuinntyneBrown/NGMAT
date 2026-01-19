import { Meta, StoryObj } from '@storybook/angular';
import { StatusDot } from './status-dot';

const meta: Meta<StatusDot> = {
  title: 'Components/Status/StatusDot',
  component: StatusDot,
  tags: ['autodocs'],
  argTypes: {
    status: {
      control: 'select',
      options: ['success', 'warning', 'error', 'info'],
    },
    pulse: {
      control: 'boolean',
    },
  },
};

export default meta;
type Story = StoryObj<StatusDot>;

export const Success: Story = {
  args: {
    status: 'success',
    pulse: false,
  },
};

export const Warning: Story = {
  args: {
    status: 'warning',
    pulse: false,
  },
};

export const Error: Story = {
  args: {
    status: 'error',
    pulse: false,
  },
};

export const Info: Story = {
  args: {
    status: 'info',
    pulse: false,
  },
};

export const Pulsing: Story = {
  args: {
    status: 'success',
    pulse: true,
  },
};
