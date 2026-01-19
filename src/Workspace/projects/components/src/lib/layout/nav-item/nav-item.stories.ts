import type { Meta, StoryObj } from '@storybook/angular';
import { NavItem } from './nav-item';

const meta: Meta<NavItem> = {
  title: 'Components/Layout/NavItem',
  component: NavItem,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Label text for the navigation item',
    },
    icon: {
      control: 'text',
      description: 'Material icon name',
    },
    active: {
      control: 'boolean',
      description: 'Whether the item is currently active',
    },
    routerLink: {
      control: 'text',
      description: 'Router link path',
    },
    badge: {
      control: 'text',
      description: 'Badge content (number or text)',
    },
    disabled: {
      control: 'boolean',
      description: 'Whether the item is disabled',
    },
    collapsed: {
      control: 'boolean',
      description: 'Whether to show only icon (collapsed sidenav)',
    },
  },
};

export default meta;
type Story = StoryObj<NavItem>;

export const Default: Story = {
  args: {
    label: 'Dashboard',
    icon: 'dashboard',
    active: false,
    disabled: false,
    collapsed: false,
  },
};

export const Active: Story = {
  args: {
    label: 'Dashboard',
    icon: 'dashboard',
    active: true,
    disabled: false,
    collapsed: false,
  },
};

export const WithBadge: Story = {
  args: {
    label: 'Notifications',
    icon: 'notifications',
    active: false,
    badge: '5',
    disabled: false,
    collapsed: false,
  },
};

export const Disabled: Story = {
  args: {
    label: 'Settings',
    icon: 'settings',
    active: false,
    disabled: true,
    collapsed: false,
  },
};

export const Collapsed: Story = {
  args: {
    label: 'Dashboard',
    icon: 'dashboard',
    active: false,
    disabled: false,
    collapsed: true,
  },
};

export const CollapsedWithBadge: Story = {
  args: {
    label: 'Notifications',
    icon: 'notifications',
    active: false,
    badge: '3',
    disabled: false,
    collapsed: true,
  },
};

export const NavigationList: Story = {
  render: () => ({
    template: `
      <div style="width: 256px; border: 1px solid #ccc; padding: 8px 0;">
        <g-nav-item label="Dashboard" icon="dashboard" [active]="true"></g-nav-item>
        <g-nav-item label="Missions" icon="rocket_launch"></g-nav-item>
        <g-nav-item label="Satellites" icon="satellite_alt"></g-nav-item>
        <g-nav-item label="Telemetry" icon="analytics" badge="12"></g-nav-item>
        <g-nav-item label="Reports" icon="assessment"></g-nav-item>
        <g-nav-item label="Settings" icon="settings" [disabled]="true"></g-nav-item>
      </div>
    `,
  }),
};

export const CollapsedNavigationList: Story = {
  render: () => ({
    template: `
      <div style="width: 64px; border: 1px solid #ccc; padding: 8px 0;">
        <g-nav-item label="Dashboard" icon="dashboard" [active]="true" [collapsed]="true"></g-nav-item>
        <g-nav-item label="Missions" icon="rocket_launch" [collapsed]="true"></g-nav-item>
        <g-nav-item label="Satellites" icon="satellite_alt" [collapsed]="true"></g-nav-item>
        <g-nav-item label="Telemetry" icon="analytics" badge="12" [collapsed]="true"></g-nav-item>
        <g-nav-item label="Reports" icon="assessment" [collapsed]="true"></g-nav-item>
      </div>
    `,
  }),
};
