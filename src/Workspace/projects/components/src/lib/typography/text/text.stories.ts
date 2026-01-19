import type { Meta, StoryObj } from '@storybook/angular';
import { Text } from './text';

const meta: Meta<Text> = {
  title: 'Components/Typography/Text',
  component: Text,
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: { type: 'select' },
      options: ['body-1', 'body-2', 'caption', 'overline'],
      description: 'Text variant style',
    },
    color: {
      control: { type: 'select' },
      options: ['primary', 'secondary', 'disabled', 'hint', 'inherit'],
      description: 'Text color',
    },
  },
};

export default meta;
type Story = StoryObj<Text>;

export const Body1: Story = {
  args: {
    variant: 'body-1',
    color: 'inherit',
  },
  render: (args) => ({
    props: args,
    template: `<g-text [variant]="variant" [color]="color">Body 1 text - The quick brown fox jumps over the lazy dog.</g-text>`,
  }),
};

export const Body2: Story = {
  args: {
    variant: 'body-2',
    color: 'inherit',
  },
  render: (args) => ({
    props: args,
    template: `<g-text [variant]="variant" [color]="color">Body 2 text - The quick brown fox jumps over the lazy dog.</g-text>`,
  }),
};

export const Caption: Story = {
  args: {
    variant: 'caption',
    color: 'inherit',
  },
  render: (args) => ({
    props: args,
    template: `<g-text [variant]="variant" [color]="color">Caption text - The quick brown fox jumps over the lazy dog.</g-text>`,
  }),
};

export const Overline: Story = {
  args: {
    variant: 'overline',
    color: 'inherit',
  },
  render: (args) => ({
    props: args,
    template: `<g-text [variant]="variant" [color]="color">Overline text</g-text>`,
  }),
};

export const AllVariants: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 12px;">
        <g-text variant="body-1">Body 1 - Primary body text for longer content and paragraphs.</g-text>
        <g-text variant="body-2">Body 2 - Secondary body text for smaller content blocks.</g-text>
        <g-text variant="caption">Caption - Used for annotations and helper text.</g-text>
        <g-text variant="overline">Overline - Labels and categories</g-text>
      </div>
    `,
  }),
};

export const Colors: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 8px;">
        <g-text variant="body-1" color="primary">Primary color text</g-text>
        <g-text variant="body-1" color="secondary">Secondary color text</g-text>
        <g-text variant="body-1" color="hint">Hint color text</g-text>
        <g-text variant="body-1" color="disabled">Disabled color text</g-text>
      </div>
    `,
  }),
};

export const MixedContent: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 16px;">
        <div>
          <g-text variant="overline" color="secondary">Mission Status</g-text>
        </div>
        <g-text variant="body-1">
          The spacecraft is currently in a stable orbit around Earth.
          All systems are nominal and functioning within expected parameters.
        </g-text>
        <g-text variant="caption" color="hint">Last updated: 2 minutes ago</g-text>
      </div>
    `,
  }),
};
