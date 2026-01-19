import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatCardModule } from '@angular/material/card';
import { ChartCard } from './chart-card';

const meta: Meta<ChartCard> = {
  title: 'Components/Visualization/ChartCard',
  component: ChartCard,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatCardModule],
    }),
  ],
  argTypes: {
    title: {
      control: 'text',
    },
    subtitle: {
      control: 'text',
    },
  },
};

export default meta;
type Story = StoryObj<ChartCard>;

export const Default: Story = {
  args: {
    title: 'Chart Title',
    subtitle: 'Chart subtitle with additional context',
  },
  render: (args) => ({
    props: args,
    template: `
      <g-chart-card [title]="title" [subtitle]="subtitle">
        <div style="height: 200px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 4px; display: flex; align-items: center; justify-content: center; color: white;">
          Chart Content Area
        </div>
      </g-chart-card>
    `,
  }),
};

export const TitleOnly: Story = {
  args: {
    title: 'Orbital Position Over Time',
    subtitle: '',
  },
  render: (args) => ({
    props: args,
    template: `
      <g-chart-card [title]="title" [subtitle]="subtitle">
        <div style="height: 250px; background: #f5f5f5; border-radius: 4px; display: flex; align-items: center; justify-content: center;">
          Chart Content
        </div>
      </g-chart-card>
    `,
  }),
};

export const NoHeader: Story = {
  args: {
    title: '',
    subtitle: '',
  },
  render: (args) => ({
    props: args,
    template: `
      <g-chart-card [title]="title" [subtitle]="subtitle">
        <div style="height: 300px; background: #e3f2fd; border-radius: 4px; display: flex; align-items: center; justify-content: center;">
          Full Chart Area Without Header
        </div>
      </g-chart-card>
    `,
  }),
};
