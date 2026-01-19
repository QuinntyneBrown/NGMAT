import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatExpansionModule } from '@angular/material/expansion';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ExpansionPanel } from './expansion-panel';

const meta: Meta<ExpansionPanel> = {
  title: 'Components/Expandable/ExpansionPanel',
  component: ExpansionPanel,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatExpansionModule, BrowserAnimationsModule],
    }),
  ],
  argTypes: {
    title: {
      control: 'text',
    },
    description: {
      control: 'text',
    },
    expanded: {
      control: 'boolean',
    },
    expandedChange: { action: 'expandedChange' },
  },
};

export default meta;
type Story = StoryObj<ExpansionPanel>;

export const Default: Story = {
  args: {
    title: 'Panel Title',
    description: '',
    expanded: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-expansion-panel
        [title]="title"
        [description]="description"
        [expanded]="expanded"
        (expandedChange)="expandedChange($event)">
        <p>This is the panel content. You can put any content here.</p>
      </g-expansion-panel>
    `,
  }),
};

export const WithDescription: Story = {
  args: {
    title: 'Settings',
    description: 'Configure your preferences',
    expanded: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-expansion-panel
        [title]="title"
        [description]="description"
        [expanded]="expanded"
        (expandedChange)="expandedChange($event)">
        <p>Settings content goes here.</p>
      </g-expansion-panel>
    `,
  }),
};

export const Expanded: Story = {
  args: {
    title: 'Expanded Panel',
    description: 'This panel starts expanded',
    expanded: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-expansion-panel
        [title]="title"
        [description]="description"
        [expanded]="expanded"
        (expandedChange)="expandedChange($event)">
        <p>This content is visible by default.</p>
      </g-expansion-panel>
    `,
  }),
};

export const MultiplePanels: Story = {
  args: {
    title: 'Panel 1',
    description: '',
    expanded: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-expansion-panel title="General Settings" description="Basic configuration">
        <p>General settings content.</p>
      </g-expansion-panel>
      <g-expansion-panel title="Advanced Settings" description="Expert options">
        <p>Advanced settings content.</p>
      </g-expansion-panel>
      <g-expansion-panel title="About" description="Application information">
        <p>Version 1.0.0</p>
      </g-expansion-panel>
    `,
  }),
};
