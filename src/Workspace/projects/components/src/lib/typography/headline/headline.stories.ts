import type { Meta, StoryObj } from '@storybook/angular';
import { Headline } from './headline';

const meta: Meta<Headline> = {
  title: 'Components/Typography/Headline',
  component: Headline,
  tags: ['autodocs'],
  argTypes: {
    level: {
      control: { type: 'select' },
      options: [1, 2, 3, 4, 5, 6],
      description: 'Headline level (1-6), renders corresponding h1-h6 element',
    },
  },
};

export default meta;
type Story = StoryObj<Headline>;

export const Level1: Story = {
  args: {
    level: 1,
  },
  render: (args) => ({
    props: args,
    template: `<g-headline [level]="level">Headline Level 1</g-headline>`,
  }),
};

export const Level2: Story = {
  args: {
    level: 2,
  },
  render: (args) => ({
    props: args,
    template: `<g-headline [level]="level">Headline Level 2</g-headline>`,
  }),
};

export const Level3: Story = {
  args: {
    level: 3,
  },
  render: (args) => ({
    props: args,
    template: `<g-headline [level]="level">Headline Level 3</g-headline>`,
  }),
};

export const Level4: Story = {
  args: {
    level: 4,
  },
  render: (args) => ({
    props: args,
    template: `<g-headline [level]="level">Headline Level 4</g-headline>`,
  }),
};

export const Level5: Story = {
  args: {
    level: 5,
  },
  render: (args) => ({
    props: args,
    template: `<g-headline [level]="level">Headline Level 5</g-headline>`,
  }),
};

export const Level6: Story = {
  args: {
    level: 6,
  },
  render: (args) => ({
    props: args,
    template: `<g-headline [level]="level">Headline Level 6</g-headline>`,
  }),
};

export const AllLevels: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 16px;">
        <g-headline [level]="1">Headline Level 1 - Large</g-headline>
        <g-headline [level]="2">Headline Level 2 - Medium</g-headline>
        <g-headline [level]="3">Headline Level 3 - Small</g-headline>
        <g-headline [level]="4">Headline Level 4 - Title Large</g-headline>
        <g-headline [level]="5">Headline Level 5 - Title Medium</g-headline>
        <g-headline [level]="6">Headline Level 6 - Title Small</g-headline>
      </div>
    `,
  }),
};
