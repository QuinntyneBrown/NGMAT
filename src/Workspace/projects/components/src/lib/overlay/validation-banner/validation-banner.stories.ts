import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ValidationBanner } from './validation-banner';

const meta: Meta<ValidationBanner> = {
  title: 'Components/Overlay/ValidationBanner',
  component: ValidationBanner,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatIconModule, MatButtonModule],
    }),
  ],
  argTypes: {
    status: {
      control: 'select',
      options: ['success', 'warning', 'error', 'info'],
    },
    message: {
      control: 'text',
    },
    closable: {
      control: 'boolean',
    },
    closed: { action: 'closed' },
  },
};

export default meta;
type Story = StoryObj<ValidationBanner>;

export const Success: Story = {
  args: {
    status: 'success',
    message: 'Operation completed successfully.',
    closable: false,
  },
};

export const Warning: Story = {
  args: {
    status: 'warning',
    message: 'Please review the changes before proceeding.',
    closable: false,
  },
};

export const Error: Story = {
  args: {
    status: 'error',
    message: 'An error occurred while processing your request.',
    closable: false,
  },
};

export const Info: Story = {
  args: {
    status: 'info',
    message: 'This is an informational message.',
    closable: false,
  },
};

export const Closable: Story = {
  args: {
    status: 'info',
    message: 'This banner can be dismissed by clicking the close button.',
    closable: true,
  },
};

export const ClosableError: Story = {
  args: {
    status: 'error',
    message: 'Validation failed. Please correct the errors and try again.',
    closable: true,
  },
};

export const LongMessage: Story = {
  args: {
    status: 'warning',
    message: 'This is a longer warning message that demonstrates how the banner handles text that spans multiple lines. The banner should expand vertically to accommodate the content while maintaining proper alignment of the icon and close button.',
    closable: true,
  },
};
