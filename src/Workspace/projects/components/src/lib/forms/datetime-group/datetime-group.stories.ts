import type { Meta, StoryObj } from '@storybook/angular';
import { DatetimeGroup } from './datetime-group';

const meta: Meta<DatetimeGroup> = {
  title: 'Components/Forms/DatetimeGroup',
  component: DatetimeGroup,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'DateTime group label',
    },
  },
};

export default meta;
type Story = StoryObj<DatetimeGroup>;

export const Default: Story = {
  args: {
    label: 'Epoch Time',
  },
};

export const MissionStart: Story = {
  args: {
    label: 'Mission Start Time',
  },
};

export const ManeuverTime: Story = {
  args: {
    label: 'Maneuver Execution Time',
  },
};

export const WithoutLabel: Story = {
  args: {
    label: '',
  },
};

export const ObservationWindow: Story = {
  args: {
    label: 'Observation Window Start',
  },
};
