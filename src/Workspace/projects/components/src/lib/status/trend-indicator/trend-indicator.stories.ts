import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatIconModule } from '@angular/material/icon';
import { TrendIndicator } from './trend-indicator';

const meta: Meta<TrendIndicator> = {
  title: 'Components/Status/TrendIndicator',
  component: TrendIndicator,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatIconModule],
    }),
  ],
  argTypes: {
    direction: {
      control: 'select',
      options: ['up', 'down', 'neutral'],
    },
    value: {
      control: 'text',
    },
  },
};

export default meta;
type Story = StoryObj<TrendIndicator>;

export const Up: Story = {
  args: {
    direction: 'up',
    value: '+12.5%',
  },
};

export const Down: Story = {
  args: {
    direction: 'down',
    value: '-8.3%',
  },
};

export const Neutral: Story = {
  args: {
    direction: 'neutral',
    value: '0%',
  },
};

export const NumericValue: Story = {
  args: {
    direction: 'up',
    value: 42,
  },
};
