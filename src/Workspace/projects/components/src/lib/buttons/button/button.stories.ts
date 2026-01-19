import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Button } from './button';

const meta: Meta<Button> = {
  title: 'Components/Buttons/Button',
  component: Button,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatButtonModule, MatIconModule],
    }),
  ],
  argTypes: {
    variant: {
      control: 'select',
      options: ['primary', 'stroked', 'flat', 'basic'],
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
    icon: {
      control: 'text',
    },
    clicked: { action: 'clicked' },
  },
};

export default meta;
type Story = StoryObj<Button>;

export const Primary: Story = {
  args: {
    variant: 'primary',
    color: 'primary',
    size: 'medium',
    disabled: false,
    icon: '',
  },
  render: (args) => ({
    props: args,
    template: `<g-button [variant]="variant" [color]="color" [size]="size" [disabled]="disabled" [icon]="icon" (clicked)="clicked($event)">Button</g-button>`,
  }),
};

export const Stroked: Story = {
  args: {
    variant: 'stroked',
    color: 'primary',
    size: 'medium',
    disabled: false,
  },
  render: (args) => ({
    props: args,
    template: `<g-button [variant]="variant" [color]="color" [size]="size" [disabled]="disabled" (clicked)="clicked($event)">Stroked Button</g-button>`,
  }),
};

export const Flat: Story = {
  args: {
    variant: 'flat',
    color: 'accent',
    size: 'medium',
    disabled: false,
  },
  render: (args) => ({
    props: args,
    template: `<g-button [variant]="variant" [color]="color" [size]="size" [disabled]="disabled" (clicked)="clicked($event)">Flat Button</g-button>`,
  }),
};

export const Basic: Story = {
  args: {
    variant: 'basic',
    color: 'primary',
    size: 'medium',
    disabled: false,
  },
  render: (args) => ({
    props: args,
    template: `<g-button [variant]="variant" [color]="color" [size]="size" [disabled]="disabled" (clicked)="clicked($event)">Basic Button</g-button>`,
  }),
};

export const WithIcon: Story = {
  args: {
    variant: 'primary',
    color: 'primary',
    size: 'medium',
    disabled: false,
    icon: 'add',
  },
  render: (args) => ({
    props: args,
    template: `<g-button [variant]="variant" [color]="color" [size]="size" [disabled]="disabled" [icon]="icon" (clicked)="clicked($event)">Add Item</g-button>`,
  }),
};

export const Small: Story = {
  args: {
    variant: 'primary',
    color: 'primary',
    size: 'small',
    disabled: false,
  },
  render: (args) => ({
    props: args,
    template: `<g-button [variant]="variant" [color]="color" [size]="size" [disabled]="disabled" (clicked)="clicked($event)">Small Button</g-button>`,
  }),
};

export const Large: Story = {
  args: {
    variant: 'primary',
    color: 'primary',
    size: 'large',
    disabled: false,
  },
  render: (args) => ({
    props: args,
    template: `<g-button [variant]="variant" [color]="color" [size]="size" [disabled]="disabled" (clicked)="clicked($event)">Large Button</g-button>`,
  }),
};

export const Disabled: Story = {
  args: {
    variant: 'primary',
    color: 'primary',
    size: 'medium',
    disabled: true,
  },
  render: (args) => ({
    props: args,
    template: `<g-button [variant]="variant" [color]="color" [size]="size" [disabled]="disabled" (clicked)="clicked($event)">Disabled Button</g-button>`,
  }),
};
