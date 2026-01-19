import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ZoomControls } from './zoom-controls';

const meta: Meta<ZoomControls> = {
  title: 'Components/Visualization/ZoomControls',
  component: ZoomControls,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatButtonModule, MatIconModule, MatTooltipModule],
    }),
  ],
  argTypes: {
    level: {
      control: { type: 'range', min: 25, max: 400, step: 25 },
    },
    min: {
      control: 'number',
    },
    max: {
      control: 'number',
    },
    zoomIn: { action: 'zoomIn' },
    zoomOut: { action: 'zoomOut' },
    reset: { action: 'reset' },
  },
};

export default meta;
type Story = StoryObj<ZoomControls>;

export const Default: Story = {
  args: {
    level: 100,
    min: 25,
    max: 400,
  },
};

export const ZoomedIn: Story = {
  args: {
    level: 200,
    min: 25,
    max: 400,
  },
};

export const ZoomedOut: Story = {
  args: {
    level: 50,
    min: 25,
    max: 400,
  },
};

export const AtMinimum: Story = {
  args: {
    level: 25,
    min: 25,
    max: 400,
  },
};

export const AtMaximum: Story = {
  args: {
    level: 400,
    min: 25,
    max: 400,
  },
};

export const CustomRange: Story = {
  args: {
    level: 100,
    min: 50,
    max: 200,
  },
};
