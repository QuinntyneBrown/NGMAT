import type { Meta, StoryObj } from '@storybook/angular';
import { NavSection } from './nav-section';

const meta: Meta<NavSection> = {
  title: 'Components/Layout/NavSection',
  component: NavSection,
  tags: ['autodocs'],
  argTypes: {
    title: {
      control: 'text',
      description: 'Section title displayed above the navigation items',
    },
  },
};

export default meta;
type Story = StoryObj<NavSection>;

export const Default: Story = {
  args: {
    title: 'Navigation',
  },
  render: (args) => ({
    props: args,
    template: `
      <g-nav-section [title]="title">
        <div style="padding: 8px 16px;">Item 1</div>
        <div style="padding: 8px 16px;">Item 2</div>
        <div style="padding: 8px 16px;">Item 3</div>
      </g-nav-section>
    `,
  }),
};

export const WithoutTitle: Story = {
  args: {
    title: '',
  },
  render: (args) => ({
    props: args,
    template: `
      <g-nav-section [title]="title">
        <div style="padding: 8px 16px;">Item 1</div>
        <div style="padding: 8px 16px;">Item 2</div>
      </g-nav-section>
    `,
  }),
};

export const MultipleSections: Story = {
  render: () => ({
    template: `
      <div style="width: 256px; border: 1px solid #ccc;">
        <g-nav-section title="Mission Control">
          <div style="padding: 8px 16px;">Dashboard</div>
          <div style="padding: 8px 16px;">Active Missions</div>
          <div style="padding: 8px 16px;">Mission Planning</div>
        </g-nav-section>
        <g-nav-section title="Satellites">
          <div style="padding: 8px 16px;">Fleet Overview</div>
          <div style="padding: 8px 16px;">Telemetry</div>
          <div style="padding: 8px 16px;">Health Status</div>
        </g-nav-section>
        <g-nav-section title="Reports">
          <div style="padding: 8px 16px;">Analytics</div>
          <div style="padding: 8px 16px;">Export Data</div>
        </g-nav-section>
      </div>
    `,
  }),
};
