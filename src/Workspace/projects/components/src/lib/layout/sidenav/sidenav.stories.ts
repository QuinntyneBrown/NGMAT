import type { Meta, StoryObj } from '@storybook/angular';
import { Sidenav } from './sidenav';

const meta: Meta<Sidenav> = {
  title: 'Components/Layout/Sidenav',
  component: Sidenav,
  tags: ['autodocs'],
  argTypes: {
    opened: {
      control: 'boolean',
      description: 'Whether the sidenav is opened',
    },
    mode: {
      control: { type: 'select' },
      options: ['over', 'push', 'side'],
      description: 'Sidenav mode',
    },
    collapsed: {
      control: 'boolean',
      description: 'Whether the sidenav is collapsed (narrow width)',
    },
  },
  decorators: [
    (story) => ({
      ...story,
      template: `<div style="height: 400px; border: 1px solid #ccc;">${story().template || '<g-sidenav [opened]="opened" [mode]="mode" [collapsed]="collapsed"><div style="padding: 16px;">Sidenav Content</div><div sidenavContent style="padding: 16px;">Main Content</div></g-sidenav>'}</div>`,
    }),
  ],
};

export default meta;
type Story = StoryObj<Sidenav>;

export const Default: Story = {
  args: {
    opened: true,
    mode: 'side',
    collapsed: false,
  },
};

export const Collapsed: Story = {
  args: {
    opened: true,
    mode: 'side',
    collapsed: true,
  },
};

export const OverMode: Story = {
  args: {
    opened: true,
    mode: 'over',
    collapsed: false,
  },
};

export const PushMode: Story = {
  args: {
    opened: true,
    mode: 'push',
    collapsed: false,
  },
};

export const Closed: Story = {
  args: {
    opened: false,
    mode: 'side',
    collapsed: false,
  },
};

export const WithNavItems: Story = {
  args: {
    opened: true,
    mode: 'side',
    collapsed: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-sidenav [opened]="opened" [mode]="mode" [collapsed]="collapsed">
        <div style="padding: 16px;">
          <div style="padding: 8px 16px; cursor: pointer;">Dashboard</div>
          <div style="padding: 8px 16px; cursor: pointer;">Missions</div>
          <div style="padding: 8px 16px; cursor: pointer;">Satellites</div>
          <div style="padding: 8px 16px; cursor: pointer;">Reports</div>
        </div>
        <div sidenavContent style="padding: 16px;">
          <h2>Main Content Area</h2>
          <p>This is where the main application content would go.</p>
        </div>
      </g-sidenav>
    `,
  }),
};
