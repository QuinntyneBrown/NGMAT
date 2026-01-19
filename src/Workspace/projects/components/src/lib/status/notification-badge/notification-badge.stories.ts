import { Meta, StoryObj } from '@storybook/angular';
import { NotificationBadge } from './notification-badge';

const meta: Meta<NotificationBadge> = {
  title: 'Components/Status/NotificationBadge',
  component: NotificationBadge,
  tags: ['autodocs'],
  argTypes: {
    count: {
      control: 'number',
    },
    max: {
      control: 'number',
    },
    dot: {
      control: 'boolean',
    },
  },
};

export default meta;
type Story = StoryObj<NotificationBadge>;

export const Default: Story = {
  args: {
    count: 5,
    max: 99,
    dot: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-notification-badge [count]="count" [max]="max" [dot]="dot">
        <span style="display: inline-flex; width: 40px; height: 40px; background: #e0e0e0; border-radius: 8px; align-items: center; justify-content: center;">
          Icon
        </span>
      </g-notification-badge>
    `,
  }),
};

export const HighCount: Story = {
  args: {
    count: 150,
    max: 99,
    dot: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-notification-badge [count]="count" [max]="max" [dot]="dot">
        <span style="display: inline-flex; width: 40px; height: 40px; background: #e0e0e0; border-radius: 8px; align-items: center; justify-content: center;">
          Icon
        </span>
      </g-notification-badge>
    `,
  }),
};

export const DotOnly: Story = {
  args: {
    count: 1,
    max: 99,
    dot: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-notification-badge [count]="count" [max]="max" [dot]="dot">
        <span style="display: inline-flex; width: 40px; height: 40px; background: #e0e0e0; border-radius: 8px; align-items: center; justify-content: center;">
          Icon
        </span>
      </g-notification-badge>
    `,
  }),
};

export const ZeroCount: Story = {
  args: {
    count: 0,
    max: 99,
    dot: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-notification-badge [count]="count" [max]="max" [dot]="dot">
        <span style="display: inline-flex; width: 40px; height: 40px; background: #e0e0e0; border-radius: 8px; align-items: center; justify-content: center;">
          Icon
        </span>
      </g-notification-badge>
    `,
  }),
};
