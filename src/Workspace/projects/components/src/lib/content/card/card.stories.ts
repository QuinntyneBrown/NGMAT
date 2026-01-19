import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatCardModule } from '@angular/material/card';
import { Card } from './card';

const meta: Meta<Card> = {
  title: 'Components/Content/Card',
  component: Card,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatCardModule],
    }),
  ],
  argTypes: {
    elevated: {
      control: 'boolean',
    },
  },
};

export default meta;
type Story = StoryObj<Card>;

export const Default: Story = {
  args: {
    elevated: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-card [elevated]="elevated">
        <h3>Card Title</h3>
        <p>This is the card content. You can put any content inside a card.</p>
      </g-card>
    `,
  }),
};

export const Elevated: Story = {
  args: {
    elevated: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-card [elevated]="elevated">
        <h3>Elevated Card</h3>
        <p>This card has a shadow to give it depth.</p>
      </g-card>
    `,
  }),
};

export const WithCustomContent: Story = {
  args: {
    elevated: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-card [elevated]="elevated">
        <div style="display: flex; gap: 16px; align-items: center;">
          <div style="width: 48px; height: 48px; background: #3f51b5; border-radius: 50%;"></div>
          <div>
            <h4 style="margin: 0;">Custom Content</h4>
            <p style="margin: 4px 0 0 0; color: #666;">With flexible layout</p>
          </div>
        </div>
      </g-card>
    `,
  }),
};
