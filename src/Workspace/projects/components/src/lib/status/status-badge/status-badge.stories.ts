import { Meta, StoryObj } from '@storybook/angular';
import { StatusBadge } from './status-badge';

const meta: Meta<StatusBadge> = {
  title: 'Components/Status/StatusBadge',
  component: StatusBadge,
  tags: ['autodocs'],
  argTypes: {
    status: {
      control: 'select',
      options: ['success', 'warning', 'error', 'info'],
    },
    label: {
      control: 'text',
    },
  },
};

export default meta;
type Story = StoryObj<StatusBadge>;

export const Success: Story = {
  args: {
    status: 'success',
    label: 'Active',
  },
};

export const Warning: Story = {
  args: {
    status: 'warning',
    label: 'Pending',
  },
};

export const Error: Story = {
  args: {
    status: 'error',
    label: 'Failed',
  },
};

export const Info: Story = {
  args: {
    status: 'info',
    label: 'Processing',
  },
};
