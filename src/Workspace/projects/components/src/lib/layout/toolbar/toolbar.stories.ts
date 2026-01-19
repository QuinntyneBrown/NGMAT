import type { Meta, StoryObj } from '@storybook/angular';
import { Toolbar } from './toolbar';

const meta: Meta<Toolbar> = {
  title: 'Components/Layout/Toolbar',
  component: Toolbar,
  tags: ['autodocs'],
  argTypes: {
    title: {
      control: 'text',
      description: 'Title displayed in the toolbar',
    },
    showMenuButton: {
      control: 'boolean',
      description: 'Show the hamburger menu button',
    },
    showSearch: {
      control: 'boolean',
      description: 'Show the search button',
    },
    menuClick: {
      action: 'menuClick',
      description: 'Emitted when menu button is clicked',
    },
  },
};

export default meta;
type Story = StoryObj<Toolbar>;

export const Default: Story = {
  args: {
    title: 'NGMAT Application',
    showMenuButton: false,
    showSearch: false,
  },
};

export const WithMenuButton: Story = {
  args: {
    title: 'NGMAT Application',
    showMenuButton: true,
    showSearch: false,
  },
};

export const WithSearch: Story = {
  args: {
    title: 'NGMAT Application',
    showMenuButton: false,
    showSearch: true,
  },
};

export const FullFeatured: Story = {
  args: {
    title: 'Mission Control',
    showMenuButton: true,
    showSearch: true,
  },
};

export const WithCustomContent: Story = {
  args: {
    title: 'Satellite Tracker',
    showMenuButton: true,
    showSearch: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-toolbar [title]="title" [showMenuButton]="showMenuButton" [showSearch]="showSearch">
        <button mat-icon-button>
          <mat-icon>notifications</mat-icon>
        </button>
        <button mat-icon-button>
          <mat-icon>account_circle</mat-icon>
        </button>
      </g-toolbar>
    `,
  }),
};
