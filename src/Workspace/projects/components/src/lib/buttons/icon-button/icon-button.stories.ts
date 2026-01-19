import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { IconButton } from './icon-button';

const meta: Meta<IconButton> = {
  title: 'Components/Buttons/IconButton',
  component: IconButton,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatButtonModule, MatIconModule],
    }),
  ],
  argTypes: {
    icon: {
      control: 'text',
    },
    color: {
      control: 'select',
      options: ['primary', 'accent', 'warn'],
    },
    size: {
      control: 'select',
      options: ['small', 'medium', 'large'],
    },
    disabled: {
      control: 'boolean',
    },
    clicked: { action: 'clicked' },
  },
};

export default meta;
type Story = StoryObj<IconButton>;

export const Default: Story = {
  args: {
    icon: 'home',
    color: 'primary',
    size: 'medium',
    disabled: false,
  },
};

export const Small: Story = {
  args: {
    icon: 'close',
    color: 'primary',
    size: 'small',
    disabled: false,
  },
};

export const Large: Story = {
  args: {
    icon: 'menu',
    color: 'primary',
    size: 'large',
    disabled: false,
  },
};

export const Accent: Story = {
  args: {
    icon: 'favorite',
    color: 'accent',
    size: 'medium',
    disabled: false,
  },
};

export const Warn: Story = {
  args: {
    icon: 'delete',
    color: 'warn',
    size: 'medium',
    disabled: false,
  },
};

export const Disabled: Story = {
  args: {
    icon: 'settings',
    color: 'primary',
    size: 'medium',
    disabled: true,
  },
};
