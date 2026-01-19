import type { Meta, StoryObj } from '@storybook/angular';
import { Divider } from './divider';

const meta: Meta<Divider> = {
  title: 'Components/Utilities/Divider',
  component: Divider,
  tags: ['autodocs'],
  argTypes: {
    vertical: {
      control: 'boolean',
      description: 'Whether the divider is vertical',
    },
  },
};

export default meta;
type Story = StoryObj<Divider>;

export const Horizontal: Story = {
  args: {
    vertical: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <div style="width: 300px;">
        <p>Content above</p>
        <g-divider [vertical]="vertical"></g-divider>
        <p>Content below</p>
      </div>
    `,
  }),
};

export const Vertical: Story = {
  args: {
    vertical: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <div style="display: flex; align-items: center; height: 50px; gap: 16px;">
        <span>Left content</span>
        <g-divider [vertical]="vertical" style="height: 100%;"></g-divider>
        <span>Right content</span>
      </div>
    `,
  }),
};

export const InList: Story = {
  render: () => ({
    template: `
      <div style="width: 300px;">
        <div style="padding: 16px;">Item 1</div>
        <g-divider></g-divider>
        <div style="padding: 16px;">Item 2</div>
        <g-divider></g-divider>
        <div style="padding: 16px;">Item 3</div>
        <g-divider></g-divider>
        <div style="padding: 16px;">Item 4</div>
      </div>
    `,
  }),
};

export const InToolbar: Story = {
  render: () => ({
    template: `
      <div style="display: flex; align-items: center; gap: 12px; padding: 8px; background: #f5f5f5; border-radius: 4px;">
        <button>Action 1</button>
        <button>Action 2</button>
        <g-divider [vertical]="true" style="height: 24px;"></g-divider>
        <button>Action 3</button>
        <button>Action 4</button>
      </div>
    `,
  }),
};
